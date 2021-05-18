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

	.include "src/common/variables.s"

	@@ The EXTI 0 handler hook
	allot exti_0_handler_hook, 4

	@@ The EXTI 1 handler hook
	allot exti_1_handler_hook, 4

	@@ The EXTI 2 handler hook
	allot exti_2_handler_hook, 4

	@@ The EXTI 3 handler hook
	allot exti_3_handler_hook, 4

	@@ The EXTI 4 handler hook
	allot exti_4_handler_hook, 4

	@@ The ADC handler hook
	allot adc_handler_hook, 4

	@@ The timer 2 handler hook
	allot time_2_handler_hook, 4

	@@ The timer 3 handler hook
	allot time_3_handler_hook, 4

	@@ The timer 4 handler hook
	allot time_4_handler_hook, 4

	@@ The EXTI 9-5 handler hook
	allot exti_9_5_handler_hook, 4

	@@ the EXTI 15-10 handler hook
	allot exti_15_10_handler_hook, 4
	
