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

	.include "src/common/variables.s"

	@@ The EXTI 0 handler hook
	allot exti_0_handler_hook, 4

	@@ The EXTI 1 handler hook
	allot exti_1_handler_hook, 4

	@@ The EXTI 2 handler hook
	allot exti_2_handler_hook, 4

	@@ The EXTI 3 handler hook
	allot exti_3_handler_hook, 4

i	@@ The EXTI 4 handler hook
	allot exti_4_handler_hook, 4

	@@ The ADC handler hook
	allot adc_handler_hook, 4

	@@ The timer 2 handler hook
	allot time_2_handler_hook, 4

	@@ The timer 3 handler hook
	allot time_3_handler_hook, 4

	@@ The timer 4 handler hook
	allot time_4_handler_hook, 4

	
