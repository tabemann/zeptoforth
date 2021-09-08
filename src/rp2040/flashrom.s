@ Copyright (c) 2013 Matthias Koch
@ Copyright (c) 2019-2021 Travis Bemann
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

	.equ XIP_SSI_BASE 0x18000000
	.equ SSI_CTRLR0_OFFSET 0x00
	.equ SSI_CTRLR1_OFFSET 0x04
	.equ SSI_SSIENR_OFFSET 0x08
	.equ SSI_BAUDR_OFFSET 0x14
	.equ SSI_SR_OFFSET 0x28
	.equ SSI_SPI_CTRLR0_OFFSET 0xF4

	@ Standard 1-bit SPI serial frames (0 << 21)
	@ 32 clocks per data frame (31 << 16)
	@ Send instruction and address, receive data (3 << 8)
	.equ CTRLR0_INIT_XIP ((0 << 21) | (31 << 16) | (3 << 8))

	@ Execute a read instruction (CMD_READ << 24)
	@ 8 bit command prefix (2 << 8)
	@ 24 address and mode bits (6 << 2 )
	@ Command and address are both serial (0 << 0)
	.equ SPI_CTRLR0_INIT_XIP ((CMD_READ << 24) | (2 << 8) | (6 << 2) | (0 << 0))

	@ Continuation code
	.equ CMD_CONTINUE 0xA0

	@ Put the board into continuation state (CMD_CONTINUE << 24)
	@ Command appended to the end (0 << 8)
	@ 32 address and mode bits (8 << 2)
	@ Command and address are both serial (0 << 0)
	.equ SPI_CTRLR0_CONTINUE ((CMD_CONTINUE << 24) | (0 << 8) | (8 << 2) | (0 << 0)

	@ Enable SSI
	define_word "enable-ssi", visible_flag
_enable_ssi:
	ldr r0, =XIP_SSI_BASE
	movs r1, #1
	str r1, [r0, #SSI_SSIENR_OFFSET]
	bx lr
	end_inlined

	@ Disable SSI
	define_word "disable-ssi", visible_flag
_disable_ssi:
	ldr r0, =XIP_SSI_BASE
	movs r1, #0
	str r1, [r0, #SSI_SSIENR_OFFSET]
	bx lr
	end_inlined

	@ Wait for busy to clear
	define_word "wait-ssi-busy", visible_flag
_wait_ssi_busy:
	ldr r0, =XIP_SSI_BASE + SSI_SR_OFFSET
	movs r1, #1
1:	ldr r2, [r0]
	tst r2, r1
	bne 1b
	bx lr
	end_inlined
	
	@ Issue an SSI instruction ( c -- )
	define_word "issue-ssi", visible_flag
_issue_ssi:
	push {lr}
	bl _disable_ssi
	ldr r0, =XIP_SSI_BASE + SSI_SPI_CTRLR0_OFFSET
	ldr r1, #2 << 8 @ 8 bit command prefix
	lsls tos, tos, #24
	orrs tos, r1
	str tos, [r0]
	pull_tos
	bl _enable_ssi
	bx lr
	end_inlined

	@@ Call a ROM routine with zero parameters
	define_word "call-rom-0", visible_flag
_call_rom_0:
	push {lr}
	movs r2, #0
	movs r1, tos
	pull_tos
	ldrh r0, [r2, #0x14]
	ldrh r3, [r2, #0x18]
	blx r3
	blx r0
	pop {pc}
	end_inlined

	@@ Call a ROM routine with three parameters
	define_word "call-rom-3", visible_flag
_call_rom_3:
	push {r4, lr}
	movs r2, #0
	movs r1, tos
	pull_tos
	ldrh r0, [r2, #0x14]
	ldrh r3, [r2, #0x18]
	blx r3
	movs r3, r0
	movs r2, tos
	ldmia dp!, {r1}
	ldmia dp!, {r0}
	ldmia dp!, {r4}
	blx r3
	movs tos, r4
	pop {r4, pc}
	end_inlined

	@@ Call a ROM routine with four parameters
	define_word "call-rom-4", visible_flag
_call_rom_4:
	push {r4, r5, lr}
	movs r2, #0
	movs r1, tos
	pull_tos
	ldrh r0, [r2, #0x14]
	ldrh r3, [r2, #0x18]
	blx r3
	movs r4, r0
	movs r3, tos
	ldmia dp!, {r2}
	ldmia dp!, {r1}
	ldmia dp!, {r0}
	ldmia dp!, {r5}
	blx r4
	movs tos, r5
	pop {r4, r5, pc}
	end_inlined
	
	@@ Exception handler for flash writes where flash has already been
	@@ written
	define_word "flash-already-written", visible_flag
_store_flash_already_written:
	push {lr}
	string_ln " flash already written"
	bl _type
	pop {pc}
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

	@@ Restore the state of the QSPI flash interface
	define_internal_word "connect-qspi", visible_flag
_connect_qspi:
	push {lr}
	push_tos
	ldr tos, ='I | ('F << 8)
	bl _call_rom_0
	pop {pc}
	end_inlined

	@@ Enter XIP mode - this is very slow, but we are copying into RAM so
	@@ it doesn't matter. ( -- )
	define_internal_word "enter-xip", visible_flag
_enter_xip:	
	push {lr}
	push_tos
	ldr tos, ='C | ('X << 8)
	bl _call_rom_0
	pop {pc}
	end_inlined

	@@ Enter serial-mode and then exit XIP - flushing the cache to clear
	@@ forcing the CS pin is needed before turning to XIP ( -- )
	define_internal_word "exit-xip", visible_flag
_enter_xip:
	push {lr}
	push_tos
	ldr tos, ='E | ('X << 8)
	bl _call_rom_0
	pop {pc}
	end_inlined

	@@ Flush and enable the XIP cache and clear forcing on the CS pin ( -- )
	define_internal_word "flush-xip-cache", visible_flag
_flush_xip_cache:
	push {lr}
	push_tos
	ldr tos, ='F | ('C << 8)
	bl _call_rom_0
	pop {pc}
	end_inlined

	@@ Program a range of flash ( addr count size -- )
	@@
	@@ size and count must be a multiple of 256
	define_internal_word "program-flash", visible_flag
_program_flash:
	push {lr}
	push_tos
	ldr tos, ='R | ('P << 8)
	bl _call_rom_4
	pop {pc}
	end_inlined

	@@ Erase a range of flash ( addr count size cmd -- )
	@@
	@@ Use 0xD8 for a block erase command
	@@ size and count must be a multiple of 4096
	define_internal_word "erase-flash", visible_flag
_erase_flash:
	push {lr}
	push_tos
	ldr tos, ='R | ('E << 8)
	bl _call_rom_4
	pop {pc}
	end_inlined
	
	@@ Erase after a given address (including the sector the address is in)
	define_internal_word "erase-after", visible_flag
_erase_after:
	bx lr
	end_inlined
	
	@@ Erase all flash except for the zeptoforth runtime
	define_word "erase-all", visible_flag
_erase_all:
	bx lr
	end_inlined
		
	@@ Find the end of the flash dictionary
	define_internal_word "find-flash-end", visible_flag
_find_flash_end:
	push_tos
	ldr tos, =flash_dict_end
	ldr r1, =flash_dict_start
1:	cmp tos, r1
	beq 2f
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
	movs r0, #FF
	bics tos, r0
	ldr r0, =256
	adds tos, r0
	bx lr
	end_inlined

	@@ Find the start of the last flash word
	define_internal_word "find-last-flash-word", visible_flag
_find_last_flash_word:
	ldr r1, =0xDEADBEEF
	ldr r2, =flash_dict_start
1:	subs tos, #4
	cmp tos, r2
	beq 2f
	ldr r0, [tos]
	cmp r0, r1
	bne 1b
3:	subs tos, #4
	cmp tos, r2
	beq 2f
	ldr r0, [tos]
	cmp r0, r1
	beq 3b
	ldr tos, [tos]
	bx lr
2:	ldr tos, =kernel_end
	b 3b
	bx lr
	end_inlined

	@@ Write a byte to an address in a flash buffer
	define_word "cflash!", visible_flag
_store_flash_1:
	push {lr}
	pop {pc}
	end_inlined
	
	@@ Write a halfword to an address in one or more flash_buffers
	define_word "hflash!", visible_flag
_store_flash_2:
	push {lr}
	pop {pc}
	end_inlined

	@@ Write a word to an address in one or more flash_buffers
	define_word "flash!", visible_flag
_store_flash_4:
	push {lr}
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

	@@ Flush all the buffered flash
	define_internal_word "flush-all-flash", visible_flag
_flush_all_flash:	
	bx lr
	end_inlined

	@@ Fill flash until it is aligned to a block
	define_word "flash-align,", visible_flag
_flash_align:
	bx lr
	end_inlined

	@@ Get the flash block size in bytes
	define_word "flash-block-size", visible_flag | inlined_flag
_flash_block_size:
	push_tos
	ldr tos, =flash_block_size
	bx lr
	end_inlined

	@@ Initialize the flash buffers
	define_internal_word "init-flash-buffers", visible_flag
_init_flash_buffers:
	bx lr
	end_inlined

	.ltorg
	
