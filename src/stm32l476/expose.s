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

	.include "src/common/expose.s"

	@@ Get the EXTI-0-HANDLER-HOOK variable address
	define_word "exti-0-handler-hook", visible_flag
_exti_0_handler_hook:
	push_tos
	ldr tos, =exti_0_handler_hook
	bx lr

	@@ Get the EXTI-1-HANDLER-HOOK variable address
	define_word "exti-1-handler-hook", visible_flag
_exti_1_handler_hook:
	push_tos
	ldr tos, =exti_1_handler_hook
	bx lr

	@@ Get the EXTI-2-HANDLER-HOOK variable address
	define_word "exti-2-handler-hook", visible_flag
_exti_2_handler_hook:
	push_tos
	ldr tos, =exti_2_handler_hook
	bx lr

	@@ Get the EXTI-3-HANDLER-HOOK variable address
	define_word "exti-3-handler-hook", visible_flag
_exti_3_handler_hook:
	push_tos
	ldr tos, =exti_3_handler_hook
	bx lr

	@@ Get the EXTI-4-HANDLER-HOOK variable address
	define_word "exti-4-handler-hook", visible_flag
_exti_4_handler_hook:
	push_tos
	ldr tos, =exti_4_handler_hook
	bx lr

	@@ Get the ADC-HANDLER-HOOK variable address
	define_word "adc-handler-hook", visible_flag
_adc_handler_hook:
	push_tos
	ldr tos, =adc_handler_hook
	bx lr

	@@ Get the TIME-2-HANDLER-HOOK variable address
	define_word "time-2-handler-hook", visible_flag
_time_2_handler_hook:
	push_tos
	ldr tos, =time_2_handler_hook
	bx lr

	@@ Get the TIME-3-HANDLER-HOOK variable address
	define_word "time-3-handler-hook", visible_flag
_time_3_handler_hook:
	push_tos
	ldr tos, =time_3_handler_hook
	bx lr

	@@ Get the TIME-4-HANDLER-HOOK variable address
	define_word "time-4-handler-hook", visible_flag
_time_4_handler_hook:
	push_tos
	ldr tos, =time_4_handler_hook
	bx lr
