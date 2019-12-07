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

	.word rstack_top
	.word handle_reset+1 	; 1: the reset handler
	.word handle_fault+1	; 2: the NMI handler
	.word handle_fault+1    ; 3: the hard fault handler
	.word handle_fault+1  ; 4: the MPU fault handler
	.word handle_fault+1  ; 5: the bus fault handler
	.word handle_fault+1  ; 6: the usage fault handler
	.word 0               ; 7: reserved
	.word 0               ; 8: reserved
	.word 0               ; 9: reserved
	.word 0               ; 10: reserved
	.word handle_null+1   ; 11: SVCall handler
	.word handle_null+1   ; 12: debug handler
	.word 0               ; 13: reserved
	.word handle_null+1   ; 14: the PendSV handler
	.word handle_systick+1   ; 15: the Systick handler
	
