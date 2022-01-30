@ Copyright (c) 2013 Matthias Koch
@ Copyright (C) 2018 juju2013@github
@ Copyright (c) 2019-2022 Travis Bemann
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

	.equ NVMC, 0x4001E000

	.equ NVM_READY,			NVMC + 0x400 @ Ready Flag
	.equ NVM_CONFIG,		NVMC + 0x504 @ Configuration register
	.equ NVM_ERASEPAGE,		NVMC + 0x508 @ Register for erasing a page in code region 1
	
	@@ Write 4 bytes starting at a 16-bit-aligned address in flash
	define_word "flash!", visible_flag
_store_flash_4:
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
1:	tst tos, #0x3
	beq 1f
	bl _store_flash_4_unaligned
	pop {pc}
1:	movs r0, tos
	pull_tos
	ldr r1, =0xFFFF
	cmp tos, r1
	bne 1f
	pop {pc}
1:	ldrh r0, [tos]
	ldr r1, =0xFFFF
	cmp r0, r1
	beq 1f
	ldr tos, =_store_flash_4_already_written
	bl _raise
1:	@@ Enable write
	ldr r2, =NVM_CONFIG
	movs r3, #1
	str r3, [r2]
	@@ Write to flash
	str tos, [r0]
	@@ Wait for flash
	ldr r2, =NVM_READY
2:	ldr r3, [r2]
	tst r3, #1
	beq 2b
	@@ Lock flash
	ldr r2, =NVM_CONFIG
	movs r3, #0
	str r3, [r2]
	pull_tos
	pop {pc}
	end_inlined

	@@ Exception handler for unaligned 16-byte flash writes
	define_internal_word "flash!-unaligned", visible_flag
_store_flash_4_unaligned:
	push {lr}
	string_ln " unaligned 16-byte flash write"
	bl _type
	pop {pc}
	end_inlined
	
	@@ Exception handler for flash writes where flash has already been
	@@ written
	define_word "flash!-already-written", visible_flag
_store_flash_4_already_written:
	push {lr}
	string_ln " flash already written"
	bl _type
	pop {pc}
	end_inlined
	
	@@ Delete a 1K page of flash
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

	@@ Enable erase
	ldr r2, =NVM_CONFIG
	movs r3, #2 @ Set Erase bit
	str r3, [r2]

	@@ Set page to erase
	ldr r2,  =NVM_ERASEPAGE
	str r0, [r2]
	
	@@ Wait for Flash BUSY Flag to be cleared
	ldr r2, =NVM_READY
1:	ldr r3, [r2]
	tst r3, #1
	beq 1b
	
	@@ Lock Flash after finishing this
	ldr r2, =NVM_CONFIG
	movs r3, #0
	str r3, [r2]

2:	pop {pc}
	end_inlined
	
	@@ Exception handler for flash writes where flash has already been
	@@ written
_attempted_to_write_core_flash:
	push {lr}
	string_ln " attempted to write to core flash"
	bl _type
	pop {pc}
	end_inlined

	@@ Exception handler for flash writes past the end of flash
_attempted_to_write_past_flash_end:
	push {lr}
	string_ln " attempted to write past flash end"
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
	beq 1b
	bx lr
	end_inlined

	@@ Find the next flash block
	define_internal_word "next-flash-block", visible_flag
_next_flash_block:
	movs r0, #0x3
	bics tos, r0
	movs r0, #0x4
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
	cmp r0, r2
	bne 1b
	bx lr
	end_inlined

	@@ No flash buffers are free exception handler
	define_internal_word "no-flash-buffers-free", visible_flag
_no_flash_buffers_free:
	push {lr}
	string_ln " no flash buffers free"
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
2:	ldr r2, =0x3
	bics tos, r2
	movs r2, #16
	str tos, [r0, #flash_buffer_addr]
	movs tos, r0
	str r1, [r0], #4
	str r2, [r0]
	pop {pc}
	end_inlined

	@@ Find a flash buffer for an address
	define_internal_word "get-flash-buffer", visible_flag
_get_flash_buffer:
	push {lr}
	ldr r0, =flash_buffers_start
	ldr r1, =flash_buffers_start + (flash_buffer_size * flash_buffer_count)
	movs r2, #0x3
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
	define_word "bflash!", visible_flag
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

	@@ Write a word to an unaligned address in one or more
	@@ flash_buffers
	define_word "uflash!", visible_flag
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
	ldr tos, [r0, #flash_buffer_addr]
	bl _store_flash_4
	pop {pc}
	end_inlined

	@@ Flush writing the flash
	define_internal_word "flush-flash", visible_flag
_flush_flash:
	push {lr}
	bl _get_flash_buffer
	ldr r0, [tos, #flash_buffer_space]
	cmp r0, #4
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
	define_word "flash-align,", visible_flag
_flash_align:
	push {lr}
1:	bl _flash_here
	tst tos, #0x3
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
	
