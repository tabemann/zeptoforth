@ Copyright (c) 2013 Matthias Koch
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

vectors:
	.word rstack_top
	.word handle_reset+1 	@ 1: the reset handler
	.word handle_fault+1	@ 2: the NMI handler
	.word handle_fault+1    @ 3: the hard fault handler
	.word handle_fault+1  @ 4: the MPU fault handler
	.word handle_fault+1  @ 5: the bus fault handler
	.word handle_fault+1  @ 6: the usage fault handler
	.word 0               @ 7: reserved
	.word 0               @ 8: reserved
	.word 0               @ 9: reserved
	.word 0               @ 10: reserved
	.word handle_null+1   @ 11: SVCall handler
	.word handle_null+1   @ 12: debug handler
	.word 0               @ 13: reserved
	.word handle_null+1   @ 14: the PendSV handler
	.word handle_systick+1   @ 15: the Systick handler
	
