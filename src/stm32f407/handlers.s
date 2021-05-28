@ Copyright (c) 2020 Travis Bemann
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

	.include "src/common/handlers.s"

	@@ EXTI base address
	.equ EXTI_Base, 0x40013C00

	@@ EXTI pending register
	.equ EXTI_PR, EXTI_Base + 0x14

	@@ Initialize the handler hooks
	define_word "init-handlers", visible_flag
_init_handlers:
	push {lr}
	bl _init_common_handlers
	movs r0, #0
	ldr r1, =exti_0_handler_hook
	str r0, [r1]
	ldr r1, =exti_1_handler_hook
	str r0, [r1]
	ldr r1, =exti_2_handler_hook
	str r0, [r1]
	ldr r1, =exti_3_handler_hook
	str r0, [r1]
	ldr r1, =exti_4_handler_hook
	str r0, [r1]
	ldr r1, =adc_handler_hook
	str r0, [r1]
	ldr r1, =time_2_handler_hook
	str r0, [r1]
	ldr r1, =time_3_handler_hook
	str r0, [r1]
	ldr r1, =time_4_handler_hook
	str r0, [r1]
	ldr r1, =exti_9_5_handler_hook
	str r0, [r1]
	ldr r1, =exti_15_10_handler_hook
	str r0, [r1]
	pop {pc}
	end_inlined
	
	@@ The EXTI 0 handler
handle_exti_0:
	ldr r0, =exti_0_handler_hook
	ldr r0, [r0]
	cmp r0, #0
	beq 1f
	adds r0, #1
	bx r0
1:	bx lr
	end_inlined

	@@ The EXTI 1 handler
handle_exti_1:
	ldr r0, =exti_1_handler_hook
	ldr r0, [r0]
	cmp r0, #0
	beq 1f
	adds r0, #1
	bx r0
1:	bx lr
	end_inlined

	@@ The EXTI 2 handler
handle_exti_2:
	ldr r0, =EXTI_PR
	movs r1, #4
	str r1, [r0]
	ldr r0, =exti_2_count
	ldr r1, [r0]
	adds r1, #1
	str r1, [r0]
1:	bx lr
	end_inlined

	@@ The EXTI 3 handler
handle_exti_3:
	ldr r0, =exti_3_handler_hook
	ldr r0, [r0]
	cmp r0, #0
	beq 1f
	adds r0, #1
	bx r0
1:	bx lr
	end_inlined

	@@ The EXTI 4 handler
handle_exti_4:
	ldr r0, =exti_4_handler_hook
	ldr r0, [r0]
	cmp r0, #0
	beq 1f
	adds r0, #1
	bx r0
1:	bx lr
	end_inlined

	@@ The ADC handler
handle_adc:
	ldr r0, =adc_handler_hook
	ldr r0, [r0]
	cmp r0, #0
	beq 1f
	adds r0, #1
	bx r0
1:	bx lr
	end_inlined

	@@ The timer 2 handler
handle_time_2:
	ldr r0, =time_2_handler_hook
	ldr r0, [r0]
	cmp r0, #0
	beq 1f
	adds r0, #1
	bx r0
1:	bx lr
	end_inlined

	@@ The timer 3 handler
handle_time_3:
	ldr r0, =time_3_handler_hook
	ldr r0, [r0]
	cmp r0, #0
	beq 1f
	adds r0, #1
	bx r0
1:	bx lr
	end_inlined

	@@ The timer 4 handler
handle_time_4:
	ldr r0, =time_4_handler_hook
	ldr r0, [r0]
	cmp r0, #0
	beq 1f
	adds r0, #1
	bx r0
1:	bx lr
	end_inlined

	@@ The EXTI 9-5 handler
handle_exti_9_5:
	ldr r0, =exti_9_5_handler_hook
	ldr r0, [r0]
	cmp r0, #0
	beq 1f
	adds r0, #1
	bx r0
1:	bx lr
	end_inlined

	@@ The EXTI 15-10 handler
handle_exti_15_10:
	ldr r0, =exti_15_10_handler_hook
	ldr r0, [r0]
	cmp r0, #0
	beq 1f
	adds r0, #1
	bx r0
1:	bx lr
	end_inlined

	.ltorg
	
