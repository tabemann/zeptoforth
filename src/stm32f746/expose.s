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

	.include "src/common/expose.s"

	@@ Get the EXTI-0-HANDLER-HOOK variable address
	define_word "exti-0-handler-hook", visible_flag
_exti_0_handler_hook:
	push_tos
	ldr tos, =exti_0_handler_hook
	bx lr
	end_inlined

	@@ Get the EXTI-1-HANDLER-HOOK variable address
	define_word "exti-1-handler-hook", visible_flag
_exti_1_handler_hook:
	push_tos
	ldr tos, =exti_1_handler_hook
	bx lr
	end_inlined

	@@ Get the EXTI-2-HANDLER-HOOK variable address
	define_word "exti-2-handler-hook", visible_flag
_exti_2_handler_hook:
	push_tos
	ldr tos, =exti_2_handler_hook
	bx lr
	end_inlined

	@@ Get the EXTI-3-HANDLER-HOOK variable address
	define_word "exti-3-handler-hook", visible_flag
_exti_3_handler_hook:
	push_tos
	ldr tos, =exti_3_handler_hook
	bx lr
	end_inlined

	@@ Get the EXTI-4-HANDLER-HOOK variable address
	define_word "exti-4-handler-hook", visible_flag
_exti_4_handler_hook:
	push_tos
	ldr tos, =exti_4_handler_hook
	bx lr
	end_inlined

	@@ Get the ADC-HANDLER-HOOK variable address
	define_word "adc-handler-hook", visible_flag
_adc_handler_hook:
	push_tos
	ldr tos, =adc_handler_hook
	bx lr
	end_inlined

	@@ Get the TIME-2-HANDLER-HOOK variable address
	define_word "time-2-handler-hook", visible_flag
_time_2_handler_hook:
	push_tos
	ldr tos, =time_2_handler_hook
	bx lr
	end_inlined

	@@ Get the TIME-3-HANDLER-HOOK variable address
	define_word "time-3-handler-hook", visible_flag
_time_3_handler_hook:
	push_tos
	ldr tos, =time_3_handler_hook
	bx lr
	end_inlined

	@@ Get the TIME-4-HANDLER-HOOK variable address
	define_word "time-4-handler-hook", visible_flag
_time_4_handler_hook:
	push_tos
	ldr tos, =time_4_handler_hook
	bx lr
	end_inlined

	.ltorg
	
