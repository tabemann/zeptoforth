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

	.equ thumb2, 1
	.equ ram_start, 0x20000000
	.equ ram_end, 0x20018000
	.equ rstack_size, 0x0200
	.equ rstack_top, ram_end
	.equ stack_size, 0x0200
	.equ stack_top, ram_end - rstack_size
	.equ flash_buffers_top, stack_top - stack_size
	.equ flash_block_size, 16 @ in bytes
	.equ flash_buffer_count, 32
	.equ flash_buffer_size, flash_block_size + 8
	.equ flash_buffer_space, flash_block_size
	.equ flash_buffer_addr, flash_block_size + 4
	.equ flash_min_address, 0x00004000
	.equ flash_dict_start, 0x00004000
	.equ flash_dict_end, 0x00100000
	.equ input_buffer_size, 255
	.equ pad_offset, 256
