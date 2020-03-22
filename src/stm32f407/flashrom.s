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

.equ FLASH_BASE, 0x40023C00

.equ FLASH_ACR,     FLASH_BASE + 0x00 @ Flash Access Control Register
.equ FLASH_KEYR,    FLASH_BASE + 0x04 @ Flash Key Register
.equ FLASH_OPTKEYR, FLASH_BASE + 0x08 @ Flash Option Key Register
.equ FLASH_SR,      FLASH_BASE + 0x0C @ Flash Status Register
.equ FLASH_CR,      FLASH_BASE + 0x10 @ Flash Control Register
.equ FLASH_OPTCR,   FLASH_BASE + 0x14 @ Flash Option Control Register

	@@ Write a byte to flash
	define_word "bflash!", visible_flag
_store_flash_1:
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

	ldr r2, =flash_here
	ldr r3, [r2]
	cmp r0, r3
	ble 1f
	str r0, [r2]
	
	@ Prüfe Inhalt. Schreibe nur, wenn es NICHT -1 ist.
1:	ldr r3, =0xFF
	ands r1, r3  @ High-Halfword der Daten wegmaskieren
	cmp r1, r3
	beq 2f @ Fertig ohne zu Schreiben
	
	@ Ist an der gewünschten Stelle -1 im Speicher ?
	ldrh r2, [r0]
	cmp r2, r3
	bne 4f
	
	@ Okay, alle Proben bestanden. 
	
	@ Im STM32F4 ist der Flash-Speicher gespiegelt, die wirkliche Adresse liegt weiter hinten !
	adds r0, #0x08000000
	
	@ Bereit zum Schreiben !
	
	@ Unlock Flash Control
	ldr r2, =FLASH_KEYR
	ldr r3, =0x45670123
	str r3, [r2]
	ldr r3, =0xCDEF89AB
	str r3, [r2]
	
	@ Set size to write
	ldr r2, =FLASH_CR
	ldr r3, =0x00000001 @ 8 Bits programming
	str r3, [r2]
	
	@ Write to Flash !
	strb r1, [r0]
	
	@ Wait for Flash BUSY Flag to be cleared
	ldr r2, =FLASH_SR
	
1:  	ldr r3, [r2]
	ands r3, #0x00010000
	bne 1b
	
	@ Lock Flash after finishing this
	ldr r2, =FLASH_CR
	ldr r3, =0x80000000
	str r3, [r2]

2:	bx lr
4:	push_tos
	ldr tos, =_store_flash_already_written
	bl _raise
5:	push_tos
	ldr tos, =_attempted_to_write_core_flash
	bl _raise
6:	push_tos
	ldr tos, =_attempted_to_write_past_flash_end
	bl _raise

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

	ldr r2, =flash_here
	ldr r3, [r2]
	cmp r0, r3
	ble 1f
	str r0, [r2]
	
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
	
	@ Im STM32F4 ist der Flash-Speicher gespiegelt, die wirkliche Adresse liegt weiter hinten !
	adds r0, #0x08000000
	
	@ Bereit zum Schreiben !
	
	@ Unlock Flash Control
	ldr r2, =FLASH_KEYR
	ldr r3, =0x45670123
	str r3, [r2]
	ldr r3, =0xCDEF89AB
	str r3, [r2]
	
	@ Set size to write
	ldr r2, =FLASH_CR
	ldr r3, =0x00000101 @ 16 Bits programming
	str r3, [r2]
	
	@ Write to Flash !
	strh r1, [r0]
	
	@ Wait for Flash BUSY Flag to be cleared
	ldr r2, =FLASH_SR
	
1:  	ldr r3, [r2]
	ands r3, #0x00010000
	bne 1b
	
	@ Lock Flash after finishing this
	ldr r2, =FLASH_CR
	ldr r3, =0x80000000
	str r3, [r2]

2:	bx lr
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
	
	@@ Exception handler for unaligned flash writes
	define_word "flash!-unaligned", visible_flag
_store_flash_unaligned:
	push {lr}
	string_ln " unaligned flash write"
	bl _type
	pop {pc}
	
	@@ Exception handler for flash writes where flash has already been
	@@ written
	define_word "flash!-already-written", visible_flag
_store_flash_already_written:
	push {lr}
	string_ln " flash already written"
	bl _type
	pop {pc}
	
	@@ Delete a 2K page of flash
	define_word "erase-page", visible_flag
_erase_page:	
	push {lr}
	pull_tos @@ Temporary
@	movs r0, tos
@	pull_tos
@	
@	@@ Protect the zeptoforth runtime!
@	ldr r1, =flash_min_address
@	cmp r0, r1
@	bge 1f
@	push_tos
@	ldr tos, =_attempted_to_write_core_flash
@	bl _raise
@1:  	ldr r2, =FLASH_KEYR
@	ldr r3, =0x45670123
@	str r3, [r2]
@	ldr r3, =0xCDEF89AB
@	str r3, [r2]
@	
@	@ Enable erase
@	ldr r2, =FLASH_CR
@	movs r3, #2 @ Set Erase bit
@	str r3, [r2]
@	
@	@ page size 2048 byte
@	@ bit 10:0 byte address
@	@ bit 18:11 page number
@	@ bit 19 bank number
@	@ Set page to erase
@	@ shift down bits 19:11 -> bit 11:3
@	movs r1, #11-3
@	lsrs r0, r1      @ shift down bankNr and address address to BKER, PNB[7:0]
@	ldr r2, =0xFF8   @ bank and page mask
@	ands r0, r2      @ mask out other bits
@	movs r1 ,#2
@	orrs r0, r1      @ select page erase  
@	ldr r2, =FLASH_CR
@	strh r0, [r2]    @ write page and erase page
@	
@	@ start erasing
@	movs r0,#1     @ select start
@	ldr r2, =FLASH_CR+2 
@	strh r0, [r2]  @ start page erase
@	
@	@ Wait for Flash BUSY Flag to be cleared
@	ldr r2, =FLASH_SR+2
@1:    	ldrh r3, [r2]
@	movs r0, #1
@	ands r0, r3
@	bne 1b
@	
@	@ Lock Flash after finishing this
@	ldr r2, =FLASH_CR + 3
@	movs r3, #0x80
@	strb r3, [r2]
@	
@	@ clear cache
@	@ save old cache settings
@	ldr r2, =FLASH_ACR
@	ldr r1, [r2]
@	push {r1}
@	
@	@ turn cache off
@	ldr r0,=0x600
@	str r1, [r2]
@	
@	@ reset cache
@	ldr r0, =0x1800
@	str r1, [r2]
@	
@	@ restore flash settings
@	pop {r1}  
@	str r1, [r2]
@
	pop {pc}

	@@ Exception handler for flash writes where flash has already been
	@@ written
_attempted_to_write_core_flash:
	push {lr}
	string_ln " attempted to write to core flash"
	bl _type
	pop {pc}

	@@ Exception handler for flash writes past the end of flash
_attempted_to_write_past_flash_end:
	push {lr}
	string_ln " attempted to write past flash end"
	bl _type
	pop {pc}

	@@ Erase all flash except for the zeptoforth runtime
	define_word "erase-all", visible_flag
_erase_all:
	push {tos, lr}
@	ldr tos, =flash_dict_end
@1:	ldr r0, =2048
@	subs tos, tos, r0
@	ldr r0, =flash_min_address
@	cmp tos, r0
@	blt 2f
@	push_tos
@	bl _erase_page
@	b 1b
@2:	bl _init_flash_dict
	pop {tos, pc}

@	@@ Erase flash after a given address
	define_word "erase-after", visible_flag
_erase_after:
	push {lr}
	pull_tos @@ Temporary
@	movs r1, tos
@	ldr r0, =flash_here
@	ldr tos, [r0]
@	ldr r2, =0x7FF
@	bics tos, r2
@	ldr r0, =2048
@	adds tos, tos, r0
@1:	ldr r0, =2048
@	subs tos, tos, r0
@	cmp tos, r1
@	blt 2f
@	push_tos
@	push {r1}
@	bl _erase_page
@	pop {r1}
@	bl 1b
@2:	pull_tos
@	bl _init_flash_dict
	pop {pc}
	
	@@ Find the end of the flash dictionary
	define_word "find-flash-end", visible_flag
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

	@@ Find the next flash block
	define_word "next-flash-block", visible_flag
_next_flash_block:
	adds tos, #1
	bx lr

	@@ Find the start of the last flash word
	define_word "find-last-flash-word", visible_flag
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

	@@ Flush all the buffered flash
	define_word "flush-all-flash", visible_flag
_flush_all_flash:	
	push {lr}
	pop {pc}

	@@ Fill flash until it is aligned to a block
	define_word "flash-align,", visible_flag
_flash_align:
	push {lr}
	pop {pc}

	@@ Get the flash block size in bytes
	define_word "flash-block-size", visible_flag | inlined_flag
_flash_block_size:
	push_tos
	ldr tos, =flash_block_size
	bx lr
	end_inlined

	@@ Initialize the flash buffers
	define_word "init-flash-buffers", visible_flag
_init_flash_buffers:
	bx lr

	.ltorg
	
