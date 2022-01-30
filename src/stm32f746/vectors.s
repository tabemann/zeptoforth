@ Copyright (c) 2013 Matthias Koch
@ Copyright (c) 2019-2022 Travis Bemann
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

	.include "../common/vectors.s"
	.word _handle_null+1 @ 16: window watchdog
	.word _handle_null+1 @ 17: PVD
	.word _handle_null+1 @ 18: tamper and timestamp
	.word _handle_null+1 @ 19: RTC
	.word _handle_null+1 @ 20: flash
	.word _handle_null+1 @ 21: RCC
	.word _handle_null+1 @ 22: EXTI 0
	.word _handle_null+1 @ 23: EXTI 1
	.word _handle_null+1 @ 24: EXTI 2
	.word _handle_null+1 @ 25: EXTI 3
	.word _handle_null+1 @ 26: EXTI 4
	.word _handle_null+1 @ 27: DMA1 0
	.word _handle_null+1 @ 28: DMA1 1
	.word _handle_null+1 @ 29: DMA1 2
	.word _handle_null+1 @ 30: DMA1 3
	.word _handle_null+1 @ 31: DMA1 4
	.word _handle_null+1 @ 32: DMA1 5
	.word _handle_null+1 @ 33: DMA1 6
	.word _handle_null+1 @ 34: ADC
	.word _handle_null+1 @ 35:
	.word _handle_null+1 @ 36:
	.word _handle_null+1 @ 37:
	.word _handle_null+1 @ 38:
	.word _handle_null+1 @ 39: EXTI9_5
	.word _handle_null+1 @ 40:
	.word _handle_null+1 @ 41:
	.word _handle_null+1 @ 42:
	.word _handle_null+1 @ 43:
	.word _handle_null+1 @ 44: timer 2
	.word _handle_null+1 @ 45: timer 3
	.word _handle_null+1 @ 46: timer 4
	.word _handle_null+1 @ 47:
	.word _handle_null+1 @ 48:
	.word _handle_null+1 @ 49:
	.word _handle_null+1 @ 50:
	.word _handle_null+1 @ 51:
	.word _handle_null+1 @ 52:
	.word _handle_null+1 @ 53:
	.word _handle_null+1 @ 54:
	.word _handle_null+1 @ 55:
	.word _handle_null+1 @ 56: EXTI15_10
	.word _handle_null+1 @ 57:
	.word _handle_null+1 @ 58:
	.word _handle_null+1 @ 59:
	.word _handle_null+1 @ 60:
	.word _handle_null+1 @ 61:
	.word _handle_null+1 @ 62:
	.word _handle_null+1 @ 63:
	.word _handle_null+1 @ 64:
	.word _handle_null+1 @ 65:
	.word _handle_null+1 @ 66:
	.word _handle_null+1 @ 67:
	.word _handle_null+1 @ 68:
	.word _handle_null+1 @ 69:
	.word _handle_null+1 @ 70:
	.word _handle_null+1 @ 71:
	.word _handle_null+1 @ 72:
	.word _handle_null+1 @ 73:
	.word _handle_null+1 @ 74:
	.word _handle_null+1 @ 75:
	.word _handle_null+1 @ 76:
	.word _handle_null+1 @ 77:
	.word _handle_null+1 @ 78:
	.word _handle_null+1 @ 79:
	.word _handle_null+1 @ 80:
	.word _handle_null+1 @ 81:
	.word _handle_null+1 @ 97:
	.word _handle_null+1 @ 98:
	.word _handle_null+1 @ 99:
	.word _handle_null+1 @ 100:
	.word _handle_null+1 @ 101:
	.word _handle_null+1 @ 102:
	.word _handle_null+1 @ 103:
	.word _handle_null+1 @ 104:
	.word _handle_null+1 @ 105:
	.word _handle_null+1 @ 106:
	.word _handle_null+1 @ 107:
	.word _handle_null+1 @ 108:
	.word _handle_null+1 @ 109:
	.word _handle_null+1 @ 110:
	.word _handle_null+1 @ 111:
	.word _handle_null+1 @ 112:
	.word _handle_null+1 @ 113:
	.word _handle_null+1 @ 114:
	.word _handle_null+1 @ 115:
	.word _handle_null+1 @ 116:
	.word _handle_null+1 @ 117:
	.word _handle_null+1 @ 118:
	.word _handle_null+1 @ 119:
	.word _handle_null+1 @ 120:
	
