@ Copyright (c) 2020 Travis Bemann
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

	.include "src/common/handlers.s"

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
	pop {pc}
	
	@@ The EXTI 0 handler
handle_exti_0:
	ldr r0, =exti_0_handler_hook
	ldr r0, [r0]
	cmp r0, #0
	beq 1f
	adds r0, #1
	bx r0
1:	bx lr

	@@ The EXTI 1 handler
handle_exti_1:
	ldr r0, =exti_1_handler_hook
	ldr r0, [r0]
	cmp r0, #0
	beq 1f
	adds r0, #1
	bx r0
1:	bx lr

	@@ The EXTI 2 handler
handle_exti_2:
	ldr r0, =exti_2_handler_hook
	ldr r0, [r0]
	cmp r0, #0
	beq 1f
	adds r0, #1
	bx r0
1:	bx lr

	@@ The EXTI 3 handler
handle_exti_3:
	ldr r0, =exti_3_handler_hook
	ldr r0, [r0]
	cmp r0, #0
	beq 1f
	adds r0, #1
	bx r0
1:	bx lr

	@@ The EXTI 4 handler
handle_exti_4:
	ldr r0, =exti_4_handler_hook
	ldr r0, [r0]
	cmp r0, #0
	beq 1f
	adds r0, #1
	bx r0
1:	bx lr

	@@ The ADC handler
handle_adc:
	ldr r0, =adc_handler_hook
	ldr r0, [r0]
	cmp r0, #0
	beq 1f
	adds r0, #1
	bx r0
1:	bx lr

	@@ The timer 2 handler
handle_time_2:
	ldr r0, =time_2_handler_hook
	ldr r0, [r0]
	cmp r0, #0
	beq 1f
	adds r0, #1
	bx r0
1:	bx lr

	@@ The timer 3 handler
handle_time_3:
	ldr r0, =time_3_handler_hook
	ldr r0, [r0]
	cmp r0, #0
	beq 1f
	adds r0, #1
	bx r0
1:	bx lr

	@@ The timer 4 handler
handle_time_4:
	ldr r0, =time_4_handler_hook
	ldr r0, [r0]
	cmp r0, #0
	beq 1f
	adds r0, #1
	bx r0
1:	bx lr
