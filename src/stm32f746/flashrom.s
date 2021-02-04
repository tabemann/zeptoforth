@ Copyright (c) 2013 Matthias Koch
@ Copyright (c) 2019-2020 Travis Bemann
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

.equ FLASH_Base, 0x40023C00

.equ FLASH_ACR,     FLASH_Base + 0x00 @ Flash Access Control Register
.equ FLASH_KEYR,    FLASH_Base + 0x04 @ Flash Key Register
.equ FLASH_OPTKEYR, FLASH_Base + 0x08 @ Flash Option Key Register
.equ FLASH_SR,      FLASH_Base + 0x0C @ Flash Status Register
.equ FLASH_CR,      FLASH_Base + 0x10 @ Flash Control Register
.equ FLASH_OPTCR,   FLASH_Base + 0x14 @ Flash Option Control Register

	@@ Write a byte to flash
	define_word "bflash!", visible_flag
_store_flash_1:
	movs r0, tos
	pull_tos
	movs r1, tos
	pull_tos

	ldr r2, =flash_min_address
	cmp r0, r2
	blo 5f

	ldr r3, =flash_dict_end
	cmp r0, r3
	bhs 6f

@	ldr r2, =flash_here
@	ldr r3, [r2]
@	cmp r0, r3
@	blo 1f
@	movs r3, r0
@	adds r3, #1
@	str r3, [r2]
	
	@ Prüfe Inhalt. Schreibe nur, wenn es NICHT -1 ist.
1:	ldr r3, =0xFF
	ands r1, r3  @ High-Halfword der Daten wegmaskieren
	cmp r1, r3
	beq 2f @ Fertig ohne zu Schreiben
	
	@ Ist an der gewünschten Stelle -1 im Speicher ?
	ldrb r2, [r0]
	cmp r2, r3
	bne 4f
	
	@ Okay, alle Proben bestanden. 

	cpsid i
	
	@ Im STM32F4 ist der Flash-Speicher gespiegelt, die wirkliche Adresse liegt weiter hinten !
	bics r0, #0xFF000000
	bics r0, #0x00F00000
	adds r0, #0x08000000
	
	@ Bereit zum Schreiben !
	
	@ Unlock Flash Control
	ldr r2, =FLASH_KEYR
	ldr r3, =0x45670123
	str r3, [r2]
	ldr r3, =0xCDEF89AB
	str r3, [r2]
	
	@ Set size to write
1:	ldr r2, =FLASH_CR
	ldr r3, =0x00000001 @ 8 Bits programming
	str r3, [r2]
	ldr r3, [r2]
	ldr r2, =0x00000001
	cmp r2, r3
	bne 1b

	@ Ensure the write order
	dsb
	
	@ Write to Flash !
	strb r1, [r0]

	@ Ensure the write order
	dsb
	
	@ Wait for Flash BUSY Flag to be cleared
	ldr r2, =FLASH_SR
	
1:  	ldr r3, [r2]
	ands r3, #0x00010000
	bne 1b
	
	@ Lock Flash after finishing this
	ldr r2, =FLASH_CR
	ldr r3, =0x80000000
	str r3, [r2]

	cpsie i
	
	bx lr
4:	push_tos
	ldr tos, =_store_flash_already_written
	bl _raise
5:	push_tos
	ldr tos, =_attempted_to_write_core_flash
	bl _raise
6:	push_tos
	ldr tos, =_attempted_to_write_past_flash_end
	bl _raise
2:	bx lr
	end_inlined
	
	@@ Write a halfword to flash
	define_word "hflash!", visible_flag
_store_flash_2:
	movs r0, tos
	pull_tos
	movs r1, tos
	pull_tos

	ldr r2, =flash_min_address
	cmp r0, r2
	blt 5f

	ldr r3, =flash_dict_end
	cmp r0, r3
	bhs 6f

@	ldr r2, =flash_here
@	ldr r3, [r2]
@	cmp r0, r3
@	blo 1f
@	movs r3, r0
@	adds r3, #2
@	str r3, [r2]
	
	@ Prüfe Inhalt. Schreibe nur, wenn es NICHT -1 ist.
1:	ldr r3, =0xFFFF
	ands r1, r3  @ High-Halfword der Daten wegmaskieren
	cmp r1, r3
	beq 2f @ Fertig ohne zu Schreiben
	
	@ Prüfe die Adresse: Sie muss auf 2 gerade sein:
	ands r2, r0, #1
	cmp r2, #0
	bne 3f
	
	@ Ist an der gewünschten Stelle -1 im Speicher ?
	ldrh r2, [r0]
	cmp r2, r3
	bne 4f
	
	@ Okay, alle Proben bestanden. 

	cpsid i
	
	@ Im STM32F4 ist der Flash-Speicher gespiegelt, die wirkliche Adresse liegt weiter hinten !
	bics r0, #0xFF000000
	bics r0, #0x00F00000
	adds r0, #0x08000000
	
	@ Bereit zum Schreiben !
	
	@ Unlock Flash Control
	ldr r2, =FLASH_KEYR
	ldr r3, =0x45670123
	str r3, [r2]
	ldr r3, =0xCDEF89AB
	str r3, [r2]
	
	@ Set size to write
1:	ldr r2, =FLASH_CR
	ldr r3, =0x00000101 @ 16 Bits programming
	str r3, [r2]
	ldr r3, [r2]
	ldr r2, =0x00000101
	cmp r2, r3
	bne 1b

	@ Ensure the write order
	dsb
	
	@ Write to Flash !
	strh r1, [r0]

	@ Ensure the write order
	dsb
	
	@ Wait for Flash BUSY Flag to be cleared
	ldr r2, =FLASH_SR
	
1:  	ldr r3, [r2]
	ands r3, #0x00010000
	bne 1b
	
	@ Lock Flash after finishing this
	ldr r2, =FLASH_CR
	ldr r3, =0x80000000
	str r3, [r2]

	cpsie i
	
	bx lr
3:	push {lr}
	movs tos, r1
	ands tos, #0xFF
	push_tos
	movs tos, r0
	push {r0, r1}
	bl _store_flash_1
	pop {r0, r1}
	push_tos
	lsrs tos, r1, #8
	push_tos
	movs tos, r0
	adds tos, #1
	bl _store_flash_1
	pop {pc}
4:	push_tos
	ldr tos, =_store_flash_already_written
	bl _raise
5:	push_tos
	ldr tos, =_attempted_to_write_core_flash
	bl _raise
6:	push_tos
	ldr tos, =_attempted_to_write_past_flash_end
	bl _raise
2:	bx lr
	end_inlined
	
	@@ Exception handler for unaligned flash writes
	define_internal_word "flash!-unaligned", visible_flag
_store_flash_unaligned:
	push {lr}
	string_ln " unaligned flash write"
	bl _type
	pop {pc}
	end_inlined
	
	@@ Exception handler for flash writes where flash has already been
	@@ written
	define_word "flash!-already-written", visible_flag
_store_flash_already_written:
	push {lr}
	string_ln " flash already written"
	bl _type
	pop {pc}
	end_inlined
	
	@@ Delete a sector of lash
	define_internal_word "erase-sector", visible_flag
_erase_sector:	
	push {lr}

	cmp tos, #2   @ Nicht den Kern in Sektor 0 löschen
	blo 2f
	cmp tos, #8  @ Es gibt nur 8 Sektoren
	bhs 2f

	@ Disable the flash caches
@	ldr r0, =FLASH_ACR
@	ldr r3, [r0]
@	ldr r2, =0x1F00
@	bics r3, r2
@	str r3, [r0]

	ldr r2, =FLASH_KEYR
	ldr r3, =0x45670123
	str r3, [r2]
	ldr r3, =0xCDEF89AB
	str r3, [r2]
	
	@ Set sector to erase
	ldr r2, =FLASH_CR
	ldr r3, =0x00010002
	lsls tos, #3
	orrs r3, tos
	str r3, [r2]
	
	@ Wait for Flash BUSY Flag to be cleared
	ldr r2, =FLASH_SR
1:      ldr r3, [r2]
	ands r3, #0x00010000
	bne 1b
	
	@ Lock Flash after finishing this
	ldr r2, =FLASH_CR
	ldr r3, =0x80000000
	str r3, [r2]

	@ Reset the flash caches
@	ldr r0, =FLASH_ACR
@	ldr r3, [r0]
@	ldr r2, =0x1800
@	orrs r3, r2
@	str r3, [r0]

	@ Enable the flash caches
@	ldr r0, =FLASH_ACR
@	ldr r3, [r0]
@	ldr r2, =0x1800
@	bics r3, r2
@	ldr r2, =0x0700
@	orrs r3, r2
@	str r3, [r0]
	
2:	pull_tos
	
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

	@@ Erase a particular address
	define_internal_word "erase-address", visible_flag
_erase_address:
	push {lr}
	movs r2, tos
	pull_tos
	movs r1, tos
	pull_tos
	movs r0, tos
	pull_tos
	cmp tos, r2
	blo 1f
	cmp tos, r1
	bhi 1f
	movs tos, r0
	bl _erase_sector
	pop {pc}
1:	pull_tos
	pop {pc}
	end_inlined
	
	@@ Choose a sector to erase
	define_internal_word "choose-sector", visible_flag
_choose_sector:
	push {lr}
	push_tos
	push_tos
	movs tos, #1
	push_tos
	ldr tos, =0x0020FFFF
	push_tos
	ldr tos, =0x00208000
	bl _erase_address
	push_tos
	push_tos
	movs tos, #2
	push_tos
	ldr tos, =0x00217FFF
	push_tos
	ldr tos, =0x00210000
	bl _erase_address
	push_tos
	push_tos
	movs tos, #3
	push_tos
	ldr tos, =0x0021FFFF
	push_tos
	ldr tos, =0x00218000
	bl _erase_address
	push_tos
	push_tos
	movs tos, #4
	push_tos
	ldr tos, =0x0023FFFF
	push_tos
	ldr tos, =0x00220000
	bl _erase_address
	push_tos
	push_tos
	movs tos, #5
	push_tos
	ldr tos, =0x0027FFFF
	push_tos
	ldr tos, =0x00240000
	bl _erase_address
	push_tos
	push_tos
	movs tos, #6
	push_tos
	ldr tos, =0x002BFFFF
	push_tos
	ldr tos, =0x00280000
	bl _erase_address
	push_tos
	push_tos
	movs tos, #7
	push_tos
	ldr tos, =0x002FFFFF
	push_tos
	ldr tos, =0x002C0000
	bl _erase_address
	pop {pc}
	end_inlined

	@@ Erase after a given address (including the sector the address is in)
	define_internal_word "erase-after", visible_flag
_erase_after:
	push {lr}
	cpsid i
	ldr r0, =0xFFFFFFFF
	ldr r1, =flash_dict_end
1:	cmp tos, r1
	bhs 3f
	ldr r2, [tos]
	cmp r2, r0
	beq 2f
	push_tos
	push {r0, r1}
	bl _choose_sector
	pop {r0, r1}
2:	adds tos, #4
	b 1b
3:	bl _reboot
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
	bics tos, #3
	adds tos, #8
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

	@@ Flush all the buffered flash
	define_internal_word "flush-all-flash", visible_flag
_flush_all_flash:	
	push {lr}
	pop {pc}
	end_inlined

	@@ Fill flash until it is aligned to a block
	define_word "flash-align,", visible_flag
_flash_align:
	push {lr}
	pop {pc}
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
	
