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

	;; Drop the top of the data stack
	define_word "drop", visble_flag
_drop:	pull_tos
	bx lr

	;; Duplicate the top of the data stack
	define_word "dup", visible_flag
_dup:	push_tos
	bx lr

	;; Swap the top two places on the data stack
	define_word "swap", visible_flag
_swap:	mov r0, tos
	ldr tos, [dp]
	str r0, [dp]
	bx lr

	;; Copy the second place on the data stack onto the top of the stack,
	;; pushing the top of the data stack to the second place
	define_word "over", visible_flag
_over:	push_tos
	ldr tos, [dp, #4]
	bx lr

	;; Rotate the top three places on the data stack, so the third place
	;; moves to the first place
	define_word "rot", visible_flag
_rot:	ldr r0, [dp, #4]
	ldr r1, [dp]
	str tos, [dp]
	str r1, [dp, #4]
	mov tos, r0
	bx lr

	;; Pick a value at a specified depth on the stack
	define_word "pick", visible_flag
_pick:	lsl tos, tos, #2
	add tos, dp
	ldr tos, [tos]
	bx lr

	;; Get the HERE pointer
	define_word "here", visible_flag
_here:	ldr r0, =here
	push_tos
	ldr tos, [r0]
	bx lr

	;; Allot space in RAM
	define_word "allot", visible_flag
_allot:	ldr r0, =here
	ldr r1, [r0]
	add r1, tos
	str r1, [r0]
	pull_tos
	bx lr

	;; Get the flash HERE pointer
	define_word "flash-here", visible_flag
_flash_here:
	ldr r0, =flash_here
	push_tos
	ldr tos, [r0]
	bx lr

	;; Allot space in flash
	define_word "flash-allot", visible_flag
_flash_allot:
	ldr r0, =flash_here
	ldr r1, [r0]
	add r1, tos
	str r1, [r0]
	pull_tos
	bx lr

	;; Get either the HERE pointer or the flash HERE pointer, depending on
	;; compilation mode
	define_word "current-here", visible_flag
_current_here:
	push {lr}
	ldr r0, =compiling_to_flash
	ldr r0, [r0]
	cmp r0, #0
	bne 1f
	bl _here
	pop {pc}
1:	bl _flash_here
	pop {pc}

	;; Allot space in RAM or in flash, depending on the compilation mode
	define_word, "current-allot", visible_flag
_current_allot:
	push {lr}
	ldr r0, =compiling_to_flash
	ldr r0, [r0]
	cmp r0, #0
	bne 1f
	bl _allot
	pop {pc}
1:	bl _flash_allot
	pop {pc}
	
	;; Execute an xt
	define_word "execute", visible_flag
_execute:
	mov r0, tos
	pull_tos
	blx r0
	
	;; Exit a word
	define_word "exit", visible_flag
_exit:	add sp, #4
	pop {pc}

	;; Store a byte
	define_word "b!", visible_flag
_store_1:
	mov r0, tos
	pull_tos
	and tos, #0xFF
	strb tos, [r0]
	pull_tos
	bx lr

	;; Store a halfword
	define_word "h!", visible_flag
_store_2:
	mov r0, tos
	pull_tos
	mov r1, 0xFFFF
	and tos, r1
	strh tos, [r0]
	pull_tos
	bx lr

	;; Store a word
	define_word "!", visible_flag
_store_4:
	move r0, tos
	pull_tos
	str tos, [r0]
	pull_tos
	bx lr

	;; Store a doubleword
	define_word "2!", visible_flag
_store_8:
	mov r0, tos
	pull_tos
	str tos, [r0]
	pull_tos
	str tos, [r0, #4]
	bx lr

	;; Get a byte
	define_word "b@", visible_flag
_get_1: ldrb tos, [tos]
	bx lr

	;; Get a halfword
	define_word "h@", visible_flag
_get_2: ldrh tos, [tos]
	bx lr

	;; Get a word
	define_word "@", visible_flag
_get_4: ldr tos, [tos]
	bx lr

	;; Get a doubleword
	define_word "2@", visible_flag
_get_8:	ldr r0, [tos]
	ldr tos, [tos, #4]
	push_tos
	mov tos, r0
	bx lr

	;; Store a byte at the HERE location
	define_word "b,", visible_flag
_comma_1:
	ldr r0, =here
	ldr r1, [r0]
	and tos, #0xFF
	strb tos, [r1], #1
	str r1, [r0]
	pull_tos
	bx lr

	;; Store a halfword at the HERE location
	define_word "h,", visible_flag
_comma_2:
	ldr r0, =here
	ldr r1, [r0]
	mov r2, #0xFFFF
	and tos, r2
	strh tos, [r1], #2
	str r1, [r0]
	pull_tos
	bx lr

	;; Store a word at the HERE location
	define_word ",", visible_flag
_comma_4:
	ldr r0, =here
	ldr r1, [r0]
	str tos, [r1], #4
	str r1, [r0]
	pull_tos
	bx lr

	;; Store a doubleword at the HERE location
	define word "2,", visible_flag
_comma_8:
	ldr r0, =here
	ldr r1, [r0]
	str tos, [r1], #4
	pull_tos
	str tos, [r1], #4
	str r1, [r0]
	bx lr

	;; Store a byte at the flash HERE location
	define_word "bflash,", visible_flag
_flash_comma_1:
	push {lr}
	ldr r0, =here
	push_tos
	ldr tos, [r0]
	push {r0, tos}
	ldr r0, =_store_flash_1+1
	blx r0
	pop {r0, r1}
	add r1, #1
	str r1, [r0]
	pop {pc}

	;; Store a halfword at the flash HERE location
	define_word "hflash,", visible_flag
_flash_comma_2:
	push {lr}
	ldr r0, =here
	push_tos
	ldr tos, [r0]
	push {r0, tos}
	ldr r0, =_store_flash_2+1
	blx r0
	pop {r0, r1}
	add r1, #2
	str r1, [r0]
	pop {pc}

	;; Store a word at the flash HERE location
	define_word "flash,", visible_flag
_flash_comma_4:
	push {lr}
	ldr r0, =here
	push_tos
	ldr tos, [r0]
	push {r0, tos}
	ldr r0, =_store_flash_4+1
	blx r0
	pop {r0, r1}
	add r1, #4
	str r1, [r0]
	pop {pc}

	;; Store a doubleword at the flash HERE location
	define word "2flash,", visible_flag
_flash_comma_8:
	push {lr}
	ldr r0, =here
	push_tos
	ldr tos, [r0]
	push {r0, tos}
	ldr r0, =_store_flash_8+1
	blx r0
	pop {r0, r1}
	add r1, #8
	str r1, [r0]
	pop {pc}

	;; Store a byte to RAM or to flash
	define_word, "bcurrent!", visible_flag
_store_current_1:
	push {lr}
	ldr r0, =compiling_to_flash
	ldr r0, [r0]
	cmp r0, #0
	bne 1f
	bl _store_1
	pop {pc}
1:	bl _store_flash_1
	pop {pc}

	;; Store a halfword to RAM or to flash
	define_word, "hcurrent!", visible_flag
_store_current_2:
	push {lr}
	ldr r0, =compiling_to_flash
	ldr r0, [r0]
	cmp r0, #0
	bne 1f
	bl _store_2
	pop {pc}
1:	bl _store_flash_2
	pop {pc}

	;; Store a word to RAM or to flash
	define_word, "current!", visible_flag
_store_current_4:
	push {lr}
	ldr r0, =compiling_to_flash
	ldr r0, [r0]
	cmp r0, #0
	bne 1f
	bl _store_4
	pop {pc}
1:	bl _store_flash_4
	pop {pc}

	;; Store a doubleword to RAM or to flash
	define_word, "2current!", visible_flag
_store_current_8:
	push {lr}
	ldr r0, =compiling_to_flash
	ldr r0, [r0]
	cmp r0, #0
	bne 1f
	bl _store_8
	pop {pc}
1:	bl _store_flash_8
	pop {pc}

	;; Store a byte to the RAM or flash HERE location
	define_word, "bcurrent,", visible_flag
_current_comma_1:
	push {lr}
	ldr r0, =compiling_to_flash
	ldr r0, [r0]
	cmp r0, #0
	bne 1f
	bl _comma_1
	pop {pc}
1:	bl _flash_comma_1
	pop {pc}

	;; Store a halfword to the RAM or flash HERE location
	define_word, "hcurrent,", visible_flag
_current_comma_2:
	push {lr}
	ldr r0, =compiling_to_flash
	ldr r0, [r0]
	cmp r0, #0
	bne 1f
	bl _comma_2
	pop {pc}
1:	bl _flash_comma_2
	pop {pc}

	;; Store a word to the RAM or flash HERE location
	define_word, "current,", visible_flag
_current_comma_4:
	push {lr}
	ldr r0, =compiling_to_flash
	ldr r0, [r0]
	cmp r0, #0
	bne 1f
	bl _comma_4
	pop {pc}
1:	bl _flash_comma_4
	pop {pc}

	;; Store a doubleword to the RAM or flash HERE location
	define_word, "2current,", visible_flag
_current_comma_8:
	push {lr}
	ldr r0, =compiling_to_flash
	ldr r0, [r0]
	cmp r0, #0
	bne 1f
	bl _comma_8
	pop {pc}
1:	bl _flash_comma_8
	pop {pc}

	;; Reserve a byte at the RAM HERE location
	define_word "breserve", visible_flag
_reserve_1:
	ldr r0, =here
	ldr r1, [r0]
	push_tos
	mov tos, r1
	add r1, #1
	str r1, [r0]
	bx lr

	;; Reserve a halfword at the RAM HERE location
	define_word "hreserve", visible_flag
_reserve_2:
	ldr r0, =here
	ldr r1, [r0]
	push_tos
	mov tos, r1
	add r1, #2
	str r1, [r0]
	bx lr

	;; Reserve a word at the RAM HERE location
	define_word "reserve", visible_flag
_reserve_4:
	ldr r0, =here
	ldr r1, [r0]
	push_tos
	mov tos, r1
	add r1, #4
	str r1, [r0]
	bx lr

	;; Reserve a doubleword at the RAM HERE location
	define_word "2reserve", visible_flag
_reserve_8:
	ldr r0, =here
	ldr r1, [r0]
	push_tos
	mov tos, r1
	add r1, #8
	str r1, [r0]
	bx lr

	;; Reserve a byte at the flash HERE location
	define_word "bflash-reserve", visible_flag
_flash_reserve_1:
	ldr r0, =flash_here
	ldr r1, [r0]
	push_tos
	mov tos, r1
	add r1, #1
	str r1, [r0]
	bx lr

	;; Reserve a halfword at the flash HERE location
	define_word "hflash-reserve", visible_flag
_flash_reserve_2:
	ldr r0, =flash_here
	ldr r1, [r0]
	push_tos
	mov tos, r1
	add r1, #2
	str r1, [r0]
	bx lr

	;; Reserve a word at the flash HERE location
	define_word "flash-reserve", visible_flag
_flash_reserve_4:
	ldr r0, =flash_here
	ldr r1, [r0]
	push_tos
	mov tos, r1
	add r1, #4
	str r1, [r0]
	bx lr

	;; Reserve a doubleword at the flash HERE location
	define_word "2flash-reserve", visible_flag
_flash_reserve_8:
	ldr r0, =flash_here
	ldr r1, [r0]
	push_tos
	mov tos, r1
	add r1, #8
	str r1, [r0]
	bx lr

	;; Reserve a byte at the RAM or flash HERE location
	define_word, "bcurrent-reserve", visible_flag
_current_reserve_1:
	push {lr}
	ldr r0, =compiling_to_flash
	ldr r0, [r0]
	cmp r0, #0
	bne 1f
	bl _reserve_1
	pop {pc}
1:	bl _flash_reserve_1
	pop {pc}

	;; Reserve a halfword at the RAM or flash HERE location
	define_word, "hcurrent-reserve", visible_flag
_current_reserve_2:
	push {lr}
	ldr r0, =compiling_to_flash
	ldr r0, [r0]
	cmp r0, #0
	bne 1f
	bl _reserve_2
	pop {pc}
1:	bl _flash_reserve_2
	pop {pc}

	;; Reserve a word at the RAM or flash HERE location
	define_word, "current-reserve", visible_flag
_current_reserve_4:
	push {lr}
	ldr r0, =compiling_to_flash
	ldr r0, [r0]
	cmp r0, #0
	bne 1f
	bl _reserve_4
	pop {pc}
1:	bl _flash_reserve_4
	pop {pc}

	;; Reserve a doubleword at the RAM or flash HERE location
	define_word, "2current-reserve", visible_flag
_current_reserve_8:
	push {lr}
	ldr r0, =compiling_to_flash
	ldr r0, [r0]
	cmp r0, #0
	bne 1f
	bl _reserve_8
	pop {pc}
1:	bl _flash_reserve_8
	pop {pc}

	;; Push a value onto the return stack
	define_word ">r", visible_flag
_push_r:
	push {tos}
	pull_tos
	bx lr

	;; Pop a value off the return stack
	define_word "r>", visible_flag
_pop_r:	push_tos
	pop {tos}
	bx lr

	;; Get a value off the return stack without popping it
	define_word "r@", visible_flag
_get_r:	push_tos
	ldr tos, [sp]
	bx lr

	;; Drop a value from the return stack
	define word "rdrop", visible_flag
_rdrop:	add sp, #4
	bx lr
