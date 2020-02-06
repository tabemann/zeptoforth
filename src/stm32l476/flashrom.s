@ Copyright (c) 2013 Matthias Koch
@ Copyright (c) 2019 Travis Bemann
@
@ This program is free software: you can redistribute it and/or modify
@ it under the terms of the GNU General Public License as published by
@ the Free Software Foundation, either version 3 of the License, or
@ (at your option) any later version.
@
@ This program is distributed in the hope that it will be useful,
@ but WITHOUT ANY WARRANTY; without even the implied warranty of
@ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
@ GNU General Public License for more details.
@
@ You should have received a copy of the GNU General Public License
@ along with this program.  If not, see <http://www.gnu.org/licenses/>.

	.equ FLASH_BASE, 0x40022000	      @ Base address
	.equ FLASH_ACR, FLASH_BASE + 0x00     @ Access control register
	.equ FLASH_PDKEYR, FLASH_BASE + 0x04  @ Power-down key register
	.equ FLASH_KEYR, FLASE_BASE + 0x08    @ Key register
	.equ FLASH_OPTKEYR, FLASH_BASE + 0x0C @	Option key register
	.equ FLASH_SR, FLASH_BASE + 0x10      @ Status register
	.equ FLASH_CR, FLASH_BASE + 0x14      @ Control register
	.equ FLASH_ECCR, FLASH_BASE + 0x18    @ ECC register
	.equ FLASH_PCROP1SR, FLASH_BASE + 0x1C @ B1 PCROP start address register
	.equ FLASH_OPTR, FLASH_BASE + 0x20    @ Option register
	.equ FLASH_UNLOCK0, 0x45670123  @ Unlock constant 0
	.equ FLASH_UNLOCK1, 0xCDEF89AB  @ Unlock constant 1
	.equ FLASH_END_PROG_MASK, 0xFFFEFFFF  @ End programming mask
	.equ FLASH_LOCK, 0x80000000     @ Lock constant
	.equ FLASH_START_ERASE, 1	      @ Bit to set to start erasing
	.equ FLASH_ERASE, 2	      @ Bit to set for erasing
	.equ FLASH_PAGE_MASK, 0x0FF8	      @ Mask for setting page to erase
	.equ FLASH_LOCK_ERASE, 0x80     @ Bit to set to lock after erasing
	.equ FLASH_CACHE_OFF_MASK, 0xFFFFF9FF @ Mask for turning off the cach
	.equ FLASH_RESET_CACHE, 0x00001800    @ Constant for resetting cahe
	
	@@ Write 16 bytes starting at a 16-bit-aligned address in flash
	define_word "16flash!", visible_flag
_store_flash_16:
	push {r0, r1, r2, r3, lr}
	cmp tos, #0xf
	beq 1f
	ldr tos, =_store_flash_16_unaligned
	bl _raise
1:	ldr r0, [tos]
	adds r0, #1
	beq 1f
	ldr tos, =_store_flash_16_already_written
	bl _raise
1:	ldr r0, [tos, #0x04]
	adds r0, #1
	beq 1f
	ldr tos, =_store_flash_16_already_written
	bl _raise
1:	ldr r0, [tos, #0x08]
	adds r0, #1
	beq 1f
	ldr tos, =_store_flash_16_already_written
	bl _raise
1:	ldr r0, [tos, #0x0c]
	adds r0, #1
	beq 1f
	ldr tos, =_store_flash_16_already_written
	bl _raise
1:	ldr r0, =0x08000000
	adds tos, tos, r0
	@@ Flash needs to be unlocked
	ldr r0, =FLASH_KEYR
	ldr r1, =FLASH_UNLOCK0
	str r1, [r0]
	ldr r1, =FLASH_UNLOCK1
	str r1, [r0]
	@@ Enable writing to the flash
	ldr r0, =FLASH_CR
	movs r1, #0x01
	str r1, [r0]
	@@ Write the flash
	ldmia dp!, {r3}
	ldmia dp!, {r2}
	ldmia dp!, {r1}
	ldmia dp!, {r0}
	stmia tos!, {r0}
	stmia tos!, {r1}
	bl wait_for_flash
	stmia tos!, {r2}
	stmia tos!, {r3}
	bl wait_for_flash
	@@ Disable writing to the flash
	ldr r0, =FLASH_CR
	ldr r1, [r0]
	ldr r2, =FLASH_END_PROG_MASK
	ands r1, r2
	str r1, [r0]
	ldr r1, [r0]
	ldr r2, =FLASH_LOCK
	orrs r1, r2
	str r1, [r0]
	@@ Get the next value on the stack to put in the TOS
	pull_tos
	pop {r0, r1, r2, r3, pc}

	@@ Wait for flash opeartions to complete
wait_for_flash:
	push {r0, r1, r2}
1:	ldr r0, =FLASH_SR
	ldr r1, [r0]
	movs r2, #0x01
	ands r1, r2
	bne 1b
	pop {r0, r1, r2}
	bx lr
	
	@@ Exception handler for unaligned 16-byte flash writes
_store_flash_16_unaligned:
	string_ln " unaligned 16-byte flash write"
	bl _type
	bl _abort
	
	@@ Exception handler for flash writes where flash has already been
	@@ written
_store_flash_16_already_written:
	string_ln " flash already written"
	bl _type
	bl _abort
	
	@@ Delete a 2K page of flash
	define_word "erase-page", visible_word
_erase_page:	
	push {r0, r1, r2, r3, lr}
	@@ Protect the zeptoforth runtime!
	ldr r0, =flash_min_address
	cmp tos, r0
	bge 1f
	ldr tos, =_attempted_to_erase_core_flash
	bl _raise
1:	@@ Unlock the flash
	ldr r0, =FLASH_KEYR
	ldr r1, =FLASH_UNLOCK0
	str r1, [r0]
	ldr r1, =FLASH_UNLOCK1
	str r1, [r0]
	@@ Turn on erasing
	ldr r0, =FLASH_CR
	movs r1, #FLASH_ERASE
	str r1, [r0]
	@@ Set up page to be erased
	movs r1, #8
	lsrs tos, r1
	ldr r1, =FLASH_PAGE_MASK
	ands tos, r1
	movs r1, #FLASH_ERASE
	orrs tos, r1
	strh tos, [r0]
	@@ Start erasing the flash
	ldr r0, =FLASH_CR+2
	movs r1, #FLASH_START_ERASE
	strh r1, [r0]
	@@ Wait for the flash to be ready
	bl wait_for_flash
	@@ Lock the flash
	ldr r0, =FLASH_CR+3
	movs r1, #FLASH_LOCK_ERASE
	strb r1, [r0]
	@@ Clear the cache
	ldr r0, =FLASH_ACR
	ldr r1, [r0]
	movs r2, r1
	ldr r3, =FLASH_CACHE_OFF_MASK
	ands r1, r3
	str r1, [r0]
	ldr r3, =FLASH_RESET_CACHE
	orrs r1, r3
	str r1, [r0]
	str r2, [r0]
	pop {r0, r1, r2, r3, pc}

	@@ Exception handler for flash writes where flash has already been
	@@ written
_attempted_to_erase_core_flash:
	string_ln " attempted to write to core flash"
	bl _type
	bl _abort

	@@ Erase all flash except for the zeptoforth runtime
	define_word "erase-all", visible_flag
_erase_all:
	push {tos, lr}
	ldr tos, =flash_dict_end
1:	ldr r0, =2048
	subs tos, tos, r0
	ldr r0, =flash_dict_start
	cmp tos, r0
	blt 2f
	push_tos
	bl _erase_page
	b 1b
2:	pop {tos, pc}

	@@ Find the end of the flash dictionary
	define_word "find-flash-end", visible_flag
_find_flash_end:
	push_tos
	ldr tos, =flash_dict_end
1:	ldr r0, [tos, #-4]!
	adds r0, #1
	bne 2f
	ldr r0, [tos, #-4]!
	adds r0, #1
	bne 2f
	ldr r0, [tos, #-4]!
	adds r0, #1
	bne 2f
	ldr r0, [tos, #-4]!
	adds r0, r0, r1
	beq 1b
2:	movs r0, #0xF
	bics tos, r0
	bx lr

	@@ Find the start of the last flash word
	define_word "find-last-flash-word", visible_flag
_find_last_flash_word:
	push {lr}
	bl _find_flash_end
1:	subs tos, #4
	ldr r0, [tos]
	cmp r0, #0
	beq 1b
	subs tos, tos, r0
	pop {pc}

	@@ Initialize the flash buffers
	define_word "init-flash-buffers", visible_flag
_init_flash_buffers:
	ldr r0, =flash_buffers_start
	movs r1, #0
	ldr r2, =flash_buffers_start + (flash_buffer_size * flash_buffer_count)
1:	str r1, [r0], #4
	str r1, [r0], #4
	str r1, [r0], #4
	str r1, [r0], #4
	str r1, [r0], #4
	str r1, [r0], #4
	cmp r0, r2
	bne 1b
	bx lr

	@@ No flash buffers are free exception handler
	define_word "no-flash-buffers-free", visible_flag
_no_flash_buffers_free:
	string_ln " no flash buffers free"
	bl _type
	bl _abort
	
	@@ Find a free flash buffer for an address
	define_word "get-free-flash-buffer", visible_flag
_get_free_flash_buffer:
	push {lr}
	ldr r0, =flash_buffers_start
	movs r1, #0
	ldr r2, =flash_buffers_start + (flash_buffer_size * flash_buffer_count)
1:	ldr r3, [r0, #flash_buffer_space]
	cmp r1, r3
	beq 2f
	adds r0, #flash_buffer_size
	cmp r0, r2
	bne 1b
	ldr tos, =_no_flash_buffers_free
	bl _raise
2:	ldr r2, =0xF
	bics tos, r2
	movs r2, #16
	str tos, [r0, #flash_buffer_addr]
	movs tos, r0
	str r1, [r0], #4
	str r1, [r0], #4
	str r1, [r0], #4
	str r1, [r0], #4
	str r2, [r0]
	pop {pc}

	@@ Find a flash buffer for an address
	define_word "get-flash-buffer", visible_flag
_get_flash_buffer:
	push {lr}
	ldr r0, =flash_buffers_start
	ldr r1, =flash_buffers_start + (flash_buffer_size * flash_buffer_count)
	movs r2, #0xF
	bics tos, r2
1:	ldr r3, [r0, #flash_buffer_addr]
	cmp tos, r3
	beq 2f
	adds r0, #flash_buffer_size
	cmp r0, r2
	bne 1b
	bl _get_free_flash_buffer
	pop {pc}
2:	movs tos, r0
	pop {pc}

	@@ Write a byte to an address in a flash buffer
	define_word "bflash!", visible_flag
_store_flash_1:
	push {lr}
	push_tos
	bl _get_flash_buffer
	movs r0, tos
	pull_tos
	movs r1, tos
	ldr r2, =0x7FF
	bics r1, r2
	subs tos, tos, r1
	ldr r1, [r0, #flash_buffer_space]
	subs r1, #1
	str r1, [r0, #flash_buffer_space]
	movs r2, r0
	adds r0, r0, tos
	pull_tos
	movs r3, #0xFF
	ands tos, r3
	strb tos, [r0]
	cmp r1, #0
	beq 1f
	pull_tos
	pop {pc}
1:	movs tos, r2
	bl _store_flash_buffer
	pop {pc}
	
	@@ Write a halfword to an address in one or more flash_buffers
	define_word "hflash!", visible_flag
_store_flash_2:
	push {lr}
	movs r0, tos
	pull_tos
	push {r0, tos}
	movs r1, #0xFF
	ands tos, r1
	push_tos
	movs tos, r0
	bl _store_flash_1
	push_tos
	pop {r0, tos}
	lsrs tos, tos, #8
	movs r1, #0xFF
	ands tos, r1
	push_tos
	movs tos, r0
	adds tos, #1
	bl _store_flash_1
	pop {pc}

	@@ Write a word to an address in one or more flash_buffers
	define_word "flash!", visible_flag
_store_flash_4:
	push {lr}
	movs r0, tos
	pull_tos
	push {r0, tos}
	ldr r1, =0xFFFF
	ands tos, r1
	push_tos
	movs tos, r0
	bl _store_flash_2
	push_tos
	pop {r0, tos}
	lsrs tos, tos, #16
	ldr r1, =0xFFFF
	ands tos, r1
	push_tos
	movs tos, r0
	adds tos, #2
	bl _store_flash_2
	pop {pc}

	@@ Write a word to an address in one or more flash_buffers
	define_word "2flash!", visible_flag
_store_flash_8:
	push {lr}
	push {tos}
	bl _store_flash_4
	push_tos
	pop {tos}
	adds tos, #4
	bl _store_flash_4
	pop {pc}

	@@ Write out the current block of flash
	define_word "store-flash-buffer", visible_flag
_store_flash_buffer:
	push {lr}
	movs r0, tos
	ldr tos, [r0]
	push_tos
	ldr tos, [r0, #4]
	push_tos
	ldr tos, [r0, #8]
	push_tos
	ldr tos, [r0, #12]
	push_tos
	ldr tos, [r0, #flash_buffer_addr]
	bl _store_flash_16
	pop {pc}

	@@ Flush writing the flash
	define_word "flush-flash", visible_flag
_flush_flash:
	push {lr}
	bl _get_flash_buffer
	ldr r0, [tos, #flash_buffer_space]
	cmp r0, #16
	beq 1f
	cmp r0, #0
	beq 1f
	movs r0, #0
	str r0, [tos, #flash_buffer_space]
	bl _store_flash_buffer
	pop {pc}
1:	pull_tos
	pop {pc}

	@@ Flush all the buffered flash
	define_word "flush-all-flash", visible_flag
_flush_all_flash:	
	push {lr}
	ldr r0, =flash_buffer_start
	ldr r1, =flash_buffer_start + (flash_buffer_size * flash_buffer_count)
1:	cmp r0, r1
	bge 2f
	push_tos
	movs tos, r0
	push {r0, r1}
	bl _flush_flash
	pop {r0, r1}
	adds r0, #flash_buffer_size
	b 1b
2:	pop {pc}
