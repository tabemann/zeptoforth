@ Copyright (c) 2019-2020 Travis Bemann
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

	@@ Raise an exception with the exception type in the TOS register
	define_word "?raise", visible_flag
_raise:	cmp tos, #0
	beq 1f
	ldr r0, =handler
	ldr sp, [r0]
	ldr r1, [sp], #4 @ #2
	str r1, [r0]
	ldr dp, [sp], #4 @ #1
	pop {pc} @ #0
1:	pull_tos
	bx lr
	end_inlined

	@@ Try to see if an exception occurs
	define_word "try", visible_flag
_try:	push {lr} @ #0
	str dp, [sp, #-4]! @ #1
	ldr r0, =handler
	ldr r0, [r0]
	str r0, [sp, #-4]! @ #2
	ldr r0, =handler
	str sp, [r0]
	mov r0, tos
	adds r0, #1 @ Commented out to deal with an issue with Cutter @@@
	pull_tos
	blx r0
	pop {r1}
	ldr r0, =handler
	str r1, [r0]
	pop {r1}
	push_const 0
	pop {pc}
	end_inlined
	
	.ltorg
	
