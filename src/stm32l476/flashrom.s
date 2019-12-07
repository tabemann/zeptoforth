; Copyright (c) 2019, Travis Bemann
; All rights reserved.
; 
; Redistribution and use in source and binary forms, with or without
; modification, are permitted provided that the following conditions are met:
; 
; 1. Redistributions of source code must retain the above copyright notice,
;    this list of conditions and the following disclaimer.
; 
; 2. Redistributions in binary form must reproduce the above copyright notice,
;    this list of conditions and the following disclaimer in the documentation
;    and/or other materials provided with the distribution.
; 
; 3. Neither the name of the copyright holder nor the names of its
;    contributors may be used to endorse or promote products derived from
;    this software without specific prior written permission.
; 
; THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
; AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
; IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
; ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE
; LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
; CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
; SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
; INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
; CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
; ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
; POSSIBILITY OF SUCH DAMAGE.

	.equ FLASH_BASE, 0x40022000	      ; Base address
	.equ FLASH_ACR, FLASH_BASE + 0x00     ; Access control register
	.equ FLASH_PDKEYR, FLASH_BASE + 0x04  ; Power-down key register
	.equ FLASH_KEYR, FLASE_BASE + 0x08    ; Key register
	.equ FLASH_OPTKEYR, FLASH_BASE + 0x0C ;	Option key register
	.equ FLASH_SR, FLASH_BASE + 0x10      ; Status register
	.equ FLASH_CR, FLASH_BASE + 0x14      ; Control register
	.equ FLASH_ECCR, FLASH_BASE + 0x18    ; ECC register
	.equ FLASH_PCROP1SR, FLASH_BASE + 0x1C ; B1 PCROP start address register
	.equ FLASH_OPTR, FLASH_BASE + 0x20    ; Option register
	.equ FLASH_UNLOCK0, 0x45670123  ; Unlock constant 0
	.equ FLASH_UNLOCK1, 0xCDEF89AB  ; Unlock constant 1
	.equ FLASH_END_PROG_MASK, 0xFFFEFFFF  ; End programming mask
	.equ FLASH_LOCK, 0x80000000     ; Lock constant
	.equ FLASH_START_ERASE, 1	      ; Bit to set to start erasing
	.equ FLASH_ERASE, 2	      ; Bit to set for erasing
	.equ FLASH_PAGE_MASK, 0x0FF8	      ; Mask for setting page to erase
	.equ FLASH_LOCK_ERASE, 0x80     ; Bit to set to lock after erasing
	.equ FLASH_CACHE_OFF_MASK, 0xFFFFF9FF ; Mask for turning off the cach
	.equ FLASH_RESET_CACHE, 0x00001800    ; Constant for resetting cahe
	
	;; Write 16 bytes starting at a 16-bit-aligned address in flash
	.define_word "16flash!", visible_flag
_store_flash_16:
	push {r0, r1, r2, r3, lr}
	cmp tos, #0x0f
	beq 1f
	ldr tos, =_store_flash_16_unaligned
	ldr r0, =_raise+1
	blx r0
1:	ldr r0, [tos]
	adds r0, #1
	beq 1f
	ldr tos, =_store_flash_16_already_written
	ldr r0, =_raise+1
	blx r0
1:	ldr r0, [tos, #0x04]
	adds r0, #1
	beq 1f
	ldr tos, =_store_flash_16_already_written
	ldr r0, =_raise+1
	blx r0
1:	ldr r0, [tos, #0x08]
	adds r0, #1
	beq 1f
	ldr tos, =_store_flash_16_already_written
	ldr r0, =_raise+1
	blx r0
1:	ldr r0, [tos, #0x0c]
	adds r0, #1
	beq 1f
	ldr tos, =_store_flash_16_already_written
	ldr r0, =_raise+1
	blx r0
1:	ldr r0, =0x08000000
	add tos, r0
	;; Flash needs to be unlocked
	ldr r0, =FLASH_KEYR
	ldr r1, =FLASH_UNLOCK0
	str r1, [r0]
	ldr r1, =FLASH_UNLOCK1
	str r1, [r0]
	;; Enable writing to the flash
	ldr r0, =FLASH_CR
	mov r1, #0x01
	str r1, [r0]
	;; Write the flash
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
	;; Disable writing to the flash
	ldr r0, =FLASH_CR
	ldr r1, [r0]
	ldr r2, =FLASH_END_PROG_MASK
	and r1, r2
	str r1, [r0]
	ldr r1, [r0]
	ldr r2, =FLASH_LOCK
	orr r1, r2
	str r1, [r0]
	;; Get the next value on the stack to put in the TOS
	pull_tos
	pop {r0, r1, r2, r3, pc}

	;; Wait for flash opeartions to complete
wait_for_flash:
	push {r0, r1}
1:	ldr r0, =FLASH_SR
	ldr r1, [r0]
	ands r1, #0x01
	bne 1b
	pop {r0, r1]
	bx lr
	
	;; Exception handler for unaligned 16-byte flash writes
_store_flash_16_unaligned:
	;; Add code here
	b .
	
	;; Exception handler for flash writes where flash has already been
	;; written
_store_flash_16_already_written:
	;; Add code here
	b .

	;; Delete a 2K page of flash
	define_word "erase-page", visible_word
erase_page:	
	push {r0, r1, r2, r3, lr}
	;; Protect the zeptoforth runtime!
	ldr r0, =flash_min_address
	cmp tos, r0
	bge 1f
	ldr tos, =_attempted_to_erase_core_flash
	ldr r0, =_raise+1
	blx r0
1:	;; Unlock the flash
	ldr r0, =FLASH_KEYR
	ldr r1, =FLASH_UNLOCK0
	str r1, [r0]
	ldr r1, =FLASH_UNLOCK1
	str r1, [r0]
	;; Turn on erasing
	ldr r0, =FLASH_CR
	mov r1, #FLASH_ERASE
	str r1, [r0]
	;; Set up page to be erased
	mov r1, #8
	lsr tos, r1
	ldr r1, =FLASH_PAGE_MASK
	and tos, r1
	mov r1, #FLASH_ERASE
	orr tos, r1
	strh tos, [r0]
	;; Start erasing the flash
	ldr r0, =FLASH_CR+2
	mov r1, #FLASH_START_ERASE
	strh r1, [r0]
	;; Wait for the flash to be ready
	bl wait_for_flash
	;; Lock the flash
	ldr r0, =FLASH_CR+3
	mov r1, #FLASH_LOCK_ERASE
	strb r1, [r0]
	;; Clear the cache
	ldr r0, =FLASH_ACR
	ldr r1, [r0]
	mov r2, r1
	ldr r3, =FLASH_CACHE_OFF_MASK
	and r1, r3
	str r1, [r0]
	ldr r3, =FLASH_RESET_CACHE
	orr r1, r3
	str r1, [r0]
	str r2, [r0]

	pop {r0, r1, r2, r3, pc}

	;; Exception handler for flash writes where flash has already been
	;; written
_attempted_to_erase_core_flash:
	;; Add code here
	b .

	;; Erase all flash except for the zeptoforth runtime
	define_word "erase-all", visible_flag
_erase_all:
	push {tos, lr}
	ldr tos, =flash_dict_end
1:	ldr r0, =2048
	sub tos, r0
	ldr r0, =flash_dict_start
	cmp tos, r0
	blt 2f
	push_tos
	ldr r0, =_erase_page+1
	blx r0
	b 1b
2:	pop {tos, pc}

	;; Find the end of the flash dictionary
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
	adds r0, r1
	beq 1b
2:	bic tos, #0x0F
	bx lr

	;; Find the start of the last flash word
	define_word "find-last-flash-word", visible_flag
_find_last_flash_word:
	push (lr}
	bl _find_flash_end
1:	ldrh r0, [tos, #-2]!
	cmp r0, #0
	beq 1b
	push_tos
	sub tos, r0
	add tos, r1
	pop {pc}

	;; Write a byte to flash
	define_word "cflash!", visible_flag
_store_flash_1:
	push {lr}
	ldr r0, =flash_buffer
	ldr r1, =flash_buffer_offset
	ldr r2, [r1]
	and tos, #0xFF
	strb tos, [r0, r1]
	pull_tos
	adds r2, #1
	str r2, [r1]
	cmp r2, #flash_block_size
	bne 1f
	bl _store_flash_current_block
1:	pop {pc}

	;; Write a halfword to flash
	define_word "hflash!", visible_flag
_store_flash_2:
	push {lr}
	push_tos
	and tos, #0xFF
	bl _store_flash_1
	lsr tos, #8
	and tos, #0xFF
	bl _store_flash_1
	pop {pc}

	;; Write a word to flash
	define_word "flash!", visible_flag
_store_flash_4:
	push {lr}
	push_tos
	and tos, #0xFF
	bl _store_flash_1
	lsr tos, #8
	push_tos
	and tos, #0xFF
	bl _store_flash_1
	lsr tos, #8
	push_tos
	and tos, #0xFF
	bl _store_flash_1
	lsr tos, #8
	and tos, #0xFF
	bl _store_flash_1
	pop {pc}

	;; Write out the current block of flash
	define_word "store-flash-block", visible_flag
_store_flash_block:
	push {lr}
	ldr r0, =flash_buffer
	ldr r1, =flash_buffer_offset
	mov r2, #0
	str r2, [r1]
	push_tos
	ldr tos, [r0]
	push_tos
	ldr tos, [r0, #4]
	push_tos
	ldr tos, [r0, #8]
	push_tos
	ldr tos, [r0, #12]
	push_tos
	ldr r0, =flash_buffer_addr
	ldr tos, [r0]
	bl _store_flash_16
	ldr r0, =flash_buffer_addr
	ldr r1, [r0]
	add r1¸ #flash_block_size
	str r1, [r0]
	pop {pc}

	;; Flush writing the flash
	define_word "flush-flash", visible_flag
_flush_flash:
	push {lr}
1:	ldr r0, =flash_buffer_offset
	ldr r0, [r0]
	cmp r0, #0
	beq 2f
	push_tos
	mov tos, #0
	bl _store_flash_1
	b 1b
2:	pop {pc}
