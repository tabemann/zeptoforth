@ Copyright (c) 2013 Matthias Koch
@ Copyright (c) 2019-2020 Travis Bemann
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
	.word handle_svcall+1   @ 11: SVCall handler
	.word handle_null+1   @ 12: debug handler
	.word 0               @ 13: reserved
	.word handle_pendsv+1   @ 14: the PendSV handler
	.word handle_systick+1  @ 15: the Systick handler
	
