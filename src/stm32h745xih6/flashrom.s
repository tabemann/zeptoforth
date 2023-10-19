@ Copyright (c) 2013 Matthias Koch
@ Copyright (c) 2019-2023 Travis Bemann
@
@ Permission is hereby granted, free of charge, to any person obtaining a copy
@ of this software and associated documentation files (the "Software"), to deal
@ in the Software without restriction, including without limitation the rights
@ to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
@ copies of the Software, and to permit persons to whom the Software is
@ furnished to do so, subject to the following conditions:
@ 
@ The above copyright notice and this permission notice shall be included in
@ all copies or substantial portions of the Software.
@ 
@ THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
@ IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
@ FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
@ AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
@ LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
@ OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
@ SOFTWARE.

	.equ FLASH_Base, 0x40022000	      @ Base address
	.equ FLASH_ACR, FLASH_Base + 0x00     @ Access control register
	.equ FLASH_PDKEYR, FLASH_Base + 0x04  @ Power-down key register
	.equ FLASH_KEYR, FLASH_Base + 0x08    @ Key register
	.equ FLASH_OPTKEYR, FLASH_Base + 0x0C @	Option key register
	.equ FLASH_SR, FLASH_Base + 0x10      @ Status register
	.equ FLASH_CR, FLASH_Base + 0x14      @ Control register
	.equ FLASH_ECCR, FLASH_Base + 0x18    @ ECC register
	.equ FLASH_PCROP1SR, FLASH_Base + 0x1C @ B1 PCROP start address register
	.equ FLASH_OPTR, FLASH_Base + 0x20    @ Option register
	
	@@ Write 32 bytes starting at a 16-bit-aligned address in flash
	define_internal_word "32flash!", visible_flag
_store_flash_32:
	push {lr}
	ldr r0, =flash_min_address
	cmp tos, r0
	bge 1f
	ldr tos, =_attempted_to_write_core_flash
	bl _raise
1:	ldr r0, =flash_dict_end
	cmp tos, r0
	blt 1f
	ldr tos, =_attempted_to_write_past_flash_end
	bl _raise
1:	tst tos, #0xf
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
1:	ldr r0, [tosm #0x10]
	adds r0, #1
	beq 1f
	ldr tos, =_store_flash_16_already_written
	bl _raise
1:	ldr r0, [tos, #0x14]
	adds r0, #1
	beq 1f
	ldr tos, =_store_flash_16_already_written
	bl _raise
1:	ldr r0, [tos, #0x18]
	adds r0, #1
	beq 1f
	ldr tos, =_store_flash_16_already_written
	bl _raise
1:	ldr r0, [tos, #0x1c]
	adds r0, #1
	beq 1f
	ldr tos, =_store_flash_16_already_written
	bl _raise
@1:	ldr r0, =flash_here
@	ldr r1, [r0]
@	cmp tos, r1
@	bls 1f
@	movs r1, tos
@	adds r1, #16
@	str r1, [r0]
1:	ldr r0, =0x08000000
	adds tos, tos, r0
	cpsid i
	@@ Flash needs to be unlocked
	ldr r0, =FLASH_KEYR
	ldr r1, =0x45670123
	str r1, [r0]
	ldr r1, =0xCDEF89AB
	str r1, [r0]
	@@ Enable writing to the flash
	ldr r0, =FLASH_CR
	movs r1, #0x01
	str r1, [r0]
	@@ Write the flash
	ldr r3, [dp, #0]
	ldr r2, [dp, #4]
	ldr r1, [dp, #8]
	ldr r0, [dp, #12]
	adds dp, #16
	str r0, [tos, #0]
	str r1, [tos, #4]
	push {r0, r1, r2, r3}
	bl wait_for_flash
	pop {r0, r1, r2, r3}
	str r2, [tos, #8]
	str r3, [tos, #12]
	push {r0, r1, r2, r3}
	bl wait_for_flash
	pop {r0, r1, r2, r3}
	@@ Disable writing to the flash
	ldr r0, =FLASH_CR
	ldrh r1, [r0]
	movs r2, #1
	bics r1, r2
	strh r1, [r0]
	ldr r1, [r0]
	ldr r2, =0x80000000
	orrs r1, r2
	str r1, [r0]
	@@ Get the next value on the stack to put in the TOS
	pull_tos
	cpsie i
	pop {pc}
	end_inlined

	@@ Wait for flash opeartions to complete
wait_for_flash:
1:	ldr r0, =FLASH_SR+2
	ldrh r1, [r0]
	movs r2, #0x01
	ands r1, r2
	bne 1b
	bx lr
	end_inlined
	
	@@ Exception handler for unaligned 16-byte flash writes
	define_internal_word "16flash!-unaligned", visible_flag
_store_flash_16_unaligned:
	push {lr}
	string_ln "unaligned 16-byte flash write"
	bl _type
	pop {pc}
	end_inlined
	
	@@ Exception handler for flash writes where flash has already been
	@@ written
	define_word "x-flash-already-written", visible_flag
_store_flash_16_already_written:
	push {lr}
	string_ln "flash already written"
	bl _type
	pop {pc}
	end_inlined
	
	@@ Delete a 2K page of flash
	define_internal_word "erase-page", visible_flag
_erase_page:	
	push {lr}
	
	movs r0, tos
	pull_tos

	@@ Protect the zeptoforth runtime!
	ldr r1, =flash_min_address
	cmp r0, r1
	blo 2f
	ldr r1, =flash_dict_end
	cmp r0, r1
	bhs 2f

  	ldr r2, =FLASH_KEYR
	ldr r3, =0x45670123
	str r3, [r2]
	ldr r3, =0xCDEF89AB
	str r3, [r2]
	
	@ Enable erase
	ldr r2, =FLASH_CR
	movs r3, #2 @ Set Erase bit
	str r3, [r2]
	
	@ page size 2048 byte
	@ bit 10:0 byte address
	@ bit 18:11 page number
	@ bit 19 bank number
	@ Set page to erase
	@ shift down bits 19:11 -> bit 11:3
	movs r1, #11-3
	lsrs r0, r1      @ shift down bankNr and address address to BKER, PNB[7:0]
	ldr r2, =0xFF8   @ bank and page mask
	ands r0, r2      @ mask out other bits
	movs r1 ,#2
	orrs r0, r1      @ select page erase  
	ldr r2, =FLASH_CR
	strh r0, [r2]    @ write page and erase page
	
	@ start erasing
	movs r0,#1     @ select start
	ldr r2, =FLASH_CR+2 
	strh r0, [r2]  @ start page erase
	
	@ Wait for Flash BUSY Flag to be cleared
	ldr r2, =FLASH_SR+2
1:	ldrh r3, [r2]
	movs r0, #1
	ands r0, r3
	bne 1b
	
	@ Lock Flash after finishing this
	ldr r2, =FLASH_CR + 3
	movs r3, #0x80
	strb r3, [r2]
	
	@ clear cache
	@ save old cache settings
	ldr r2, =FLASH_ACR
	ldr r1, [r2]
	push {r1}
	
	@ turn cache off
	ldr r0,=0x600
	bics r1, r0
	str r1, [r2]
	
	@ reset cache
	ldr r0, =0x1800
	orrs r1, r0
	str r1, [r2]
	
	@ restore flash settings
	pop {r1}  
	str r1, [r2]

2:	pop {pc}
	end_inlined
	
	@@ Exception handler for flash writes where flash has already been
	@@ written
        define_word "x-write-core-flash", visible_flag
_attempted_to_write_core_flash:
	push {lr}
	string_ln "attempted to write to core flash"
	bl _type
	pop {pc}
	end_inlined

	@@ Exception handler for flash writes past the end of flash
        define_word "x-write-past-flash-end", visible_flag
_attempted_to_write_past_flash_end:
	push {lr}
	string_ln "attempted to write past flash end"
	bl _type
	pop {pc}
	end_inlined

	@@ Erase flash after a given address
	define_internal_word "erase-after", visible_flag
_erase_after:
	push {lr}
	movs r0, tos
	pull_tos
	ldr r1, =flash_dict_end
	ldr r2, =0xFFFF
	cpsid i
1:	ldrh r3, [r0]
	cmp r3, r2
	beq 2f
	push_tos
	movs tos, r0
	push {r0, r1, r2}
	bl _erase_page
	pop {r0, r1, r2}
2:	adds r0, #2
	cmp r0, r1
	bne 1b
	bl _reboot
	pop {pc}
	end_inlined

	@@ Erase all flash except for the zeptoforth runtime
	define_word "erase-all", visible_flag
_erase_all:
	push {lr}
	push_tos
	ldr tos, =flash_dict_start
	bl _erase_after
	pop {pc}
	end_inlined
	
	@@ Find the end of the flash dictionary
	define_internal_word "find-flash-end", visible_flag
_find_flash_end:
	push_tos
	ldr tos, =flash_dict_end
1:	subs tos, #4
	ldr r0, [tos]
	adds r0, #1
	bne 2f
	subs tos, #4
	ldr r0, [tos]
	adds r0, #1
	bne 2f
	subs tos, #4
	ldr r0, [tos]
	adds r0, #1
	bne 2f
	subs tos, #4
	ldr r0, [tos]
	adds r0, #1
	beq 1b
2:	bx lr
	end_inlined

	@@ Find the next flash block
	define_internal_word "next-flash-block", visible_flag
_next_flash_block:
	movs r0, #0xF
	bics tos, r0
	movs r0, #0x10
	adds tos, tos, r0
	bx lr
	end_inlined

	@@ Find the start of the last flash word
	define_internal_word "find-last-flash-word", visible_flag
_find_last_flash_word:
	ldr r1, =0xDEADBEEF
1:	subs tos, #4
	cmp tos, #0
	beq 2f
	ldr r0, [tos]
	cmp r0, r1
	bne 1b
3:	subs tos, #4
	cmp tos, #0
	beq 2f
	ldr r0, [tos]
	cmp r0, r1
	beq 3b
	ldr tos, [tos]
2:	bx lr
	end_inlined

	@@ Initialize the flash buffers
	define_internal_word "init-flash-buffers", visible_flag
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
	end_inlined

	@@ No flash buffers are free exception handler
	define_word "no-flash-buffers-free", visible_flag
_no_flash_buffers_free:
	push {lr}
	string_ln "no flash buffers free"
	bl _type
	pop {pc}
	end_inlined
	
	@@ Find a free flash buffer for an address
	define_internal_word "get-free-flash-buffer", visible_flag
_get_free_flash_buffer:
	push {lr}
	ldr r0, =flash_buffers_start
	ldr r2, =flash_buffers_start + (flash_buffer_size * flash_buffer_count)
1:	ldr r3, [r0, #flash_buffer_space]
	cmp r3, #0
	beq 2f
	cmp r3, #flash_block_size
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
        movs r1, #0
        mvns r1, r1
	str r1, [r0], #4
	str r1, [r0], #4
	str r1, [r0], #4
	str r1, [r0], #4
	str r2, [r0]
	pop {pc}
	end_inlined

        @@ Find an address in a flash buffer and get its value, else -1
        @@ if not in a buffer
        define_internal_word "cflash-buffer@", visible_flag
_get_flash_buffer_value_1:
	ldr r0, =flash_buffers_start
	ldr r1, =flash_buffers_start + (flash_buffer_size * flash_buffer_count)
	movs r2, #0xF
        movs r3, tos
	bics tos, r2
        movs r2, r3
1:	ldr r3, [r0, #flash_block_size]
        cmp r3, #0
        beq 3f
        cmp r3, #16
        beq 3f
        ldr r3, [r0, #flash_buffer_addr]
	cmp tos, r3
	beq 2f
3:      adds r0, #flash_buffer_size
	cmp r0, r1
	bne 1b
	ldrb tos, [r2]
        bx lr
2:      movs r3, #0xF
        ands r2, r3
        ldrb tos, [r0, r2]
        bx lr
	end_inlined

        @@ Find an address in a flash buffer and get its value, else -1
        @@ if not in a buffer
        define_internal_word "hflash-buffer@", visible_flag
_get_flash_buffer_value_2:
	ldr r0, =flash_buffers_start
	ldr r1, =flash_buffers_start + (flash_buffer_size * flash_buffer_count)
	movs r2, #0xF
        movs r3, tos
	bics tos, r2
        movs r2, r3
1:	ldr r3, [r0, #flash_block_size]
        cmp r3, #0
        beq 3f
        cmp r3, #16
        beq 3f
        ldr r3, [r0, #flash_buffer_addr]
	cmp tos, r3
	beq 2f
3:      adds r0, #flash_buffer_size
	cmp r0, r1
	bne 1b
	ldrh tos, [r2]
        bx lr
2:      movs r3, #0xF
        ands r2, r3
        ldrh tos, [r0, r2]
        bx lr
	end_inlined

        @@ Find an address in a flash buffer and get its value, else -1
        @@ if not in a buffer
        define_internal_word "flash-buffer@", visible_flag
_get_flash_buffer_value_4:
	ldr r0, =flash_buffers_start
	ldr r1, =flash_buffers_start + (flash_buffer_size * flash_buffer_count)
	movs r2, #0xF
        movs r3, tos
	bics tos, r2
        movs r2, r3
1:	ldr r3, [r0, #flash_block_size]
        cmp r3, #0
        beq 3f
        cmp r3, #16
        beq 3f
        ldr r3, [r0, #flash_buffer_addr]
	cmp tos, r3
	beq 2f
3:      adds r0, #flash_buffer_size
	cmp r0, r1
	bne 1b
	ldr tos, [r2]
        bx lr
2:      movs r3, #0xF
        ands r2, r3
        ldr tos, [r0, r2]
        bx lr
	end_inlined

	@@ Find a flash buffer for an address
	define_internal_word "get-flash-buffer", visible_flag
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
	cmp r0, r1
	bne 1b
	bl _get_free_flash_buffer
	pop {pc}
2:	movs tos, r0
	pop {pc}
	end_inlined

	@@ Write a byte to an address in a flash buffer
	define_word "cflash!", visible_flag
_store_flash_1:
	push {lr}
	push_tos
	bl _get_flash_buffer
	movs r0, tos
	pull_tos
	movs r2, #0xF
	ands tos, r2
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
	end_inlined
	
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
	end_inlined

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
	end_inlined

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
	end_inlined

	@@ Write out the current block of flash
	define_internal_word "store-flash-buffer", visible_flag
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
	end_inlined

	@@ Flush writing the flash
	define_internal_word "flush-flash", visible_flag
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
	end_inlined

	@@ Flush all the buffered flash
	define_internal_word "flush-all-flash", visible_flag
_flush_all_flash:	
	push {lr}
	ldr r0, =flash_buffers_start
	ldr r1, =flash_buffers_start + (flash_buffer_size * flash_buffer_count)
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
	end_inlined

	@@ Fill flash until it is aligned to a 16-byte block
	define_word "flash-block-align,", visible_flag
_flash_block_align:
	push {lr}
1:	bl _flash_here
	tst tos, #0xF
	beq 2f
	movs tos, #0
	bl _flash_comma_1
	b 1b
2:	pull_tos
	pop {pc}
	end_inlined

	@@ Get the flash block size in bytes
	define_word "flash-block-size", visible_flag | inlined_flag
_flash_block_size:
	push_tos
	ldr tos, =flash_block_size
	bx lr
	end_inlined
	
	.ltorg
	
