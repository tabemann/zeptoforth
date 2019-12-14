@ Copyright (c) 2019, Travis Bemann
@ All rights reserved.
@ 
@ Redistribution and use in source and binary forms, with or without
@ modification, are permitted provided that the following conditions are met:
@ 
@ 1. Redistributions of source code must retain the above copyright notice,
@    this list of conditions and the following disclaimer.
@ 
@ 2. Redistributions in binary form must reproduce the above copyright notice,
@    this list of conditions and the following disclaimer in the documentation
@    and/or other materials provided with the distribution.
@ 
@ 3. Neither the name of the copyright holder nor the names of its
@    contributors may be used to endorse or promote products derived from
@    this software without specific prior written permission.
@ 
@ THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
@ AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
@ IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
@ ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE
@ LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
@ CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
@ SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
@ INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
@ CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
@ ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
@ POSSIBILITY OF SUCH DAMAGE.
	
	.syntax	unified
	.cpu cortex-m4
	.thumb

	.include "src/stm32l476/config.s"
	.include "src/common/macro.s"
	.include "src/common/variables.s"
	
	.text

	.include "src/stm32l476/vectors.s"
	.include "src/stm32l476/use_48mhz.s"
	
	@@ The first (null) dictionary entry
	.p2align 1
	.hword invisible_flag
	.word 0
	.byte 0
	.p2align 1
	@@ The entry point
handle_reset:
	@@ Initialize the top of stack register
	ldr tos, =0xFEDCBA98
	@@ Initialize the data stack pointer
	ldr r0, =stack_top
	mov dp, r0
	@@ Just in case someone calls this we will restore the return stack
	@@ pointer.
	ldr r0, =rstack_top
	mov sp, r0
	@@ Put a deliberate garbage value in handler
	ldr r0, =0xF0E1C2D3
	ldr r1, =handler
	str r0, [r1]
	@@ Call the rest of the runtime in an exception handler
	push_tos
	ldr tos, =outer_exc_handled
	bl _try
	@@ If the inner loop returns, reboot
	b handler_reset

	@@ The outermost exception handling@ if an exception happens here the
	@@ system will reboot
outer_exc_handled:	
	bl _use_48mhz

	bl _init_flash_buffers

	.include "src/stm32l476/flashrom.s"
	@.include "src/stm32l476/console.s"
	.include "src/common/core.s"
	.include "src/common/asm.s"
	.include "src/common/exception.s"
