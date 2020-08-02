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

	@@ Display the copyright notices
	define_word "copyright", visible_flag
_copyright:
	push {lr}
	bl _cr
	string_ln "Copyright (c) 2019-2020 Travis Bemann"
	bl _type
	string_ln "Copyright (c) 2013 Matthias Koch"
	bl _type
	pop {pc}

	@@ Display the license notice
	define_word "license", visible_flag
_license:
	push {lr}
	bl _cr
	string_ln "This program is free software; you can redistribute it and/or modify"
	bl _type
	string_ln "it under the terms of the GNU General Public License as published by"
	bl _type
	string_ln "the Free Software Foundation; either version 3 of the License, or"
	bl _type
	string_ln "(at your option) any later version."
	bl _type
	bl _cr
	string_ln "This program is distributed in the hope that it will be useful,"
	bl _type
	string_ln "but WITHOUT ANY WARRANTY; without even the implied warranty of"
	bl _type
	string_ln "MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the"
	bl _type
	string_ln "GNU General Public License for more details."
	bl _type
	bl _cr
	string_ln "You should have received a copy of the GNU General Public License"
	bl _type
	string_ln "along with this program. If not, see http://www.gnu.org/licenses/."
	bl _type
	pop {pc}

	.ltorg
