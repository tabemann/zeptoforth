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
