@ Copyright (c) 2019 Travis Bemann
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

	@@ Pointer to the current HERE location
	allot here, 4

	@@ Pointer to the current Flash HERE location
	allot flash_here, 4

	@@ Flag to determine whether compilation is going to Flash
	allot compiling_to_flash, 4

	@@ Flash buffers
	allot flash_buffers_start, flash_buffer_size * flash_buffer_count	
