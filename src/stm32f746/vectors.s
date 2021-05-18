@ Copyright (c) 2013 Matthias Koch
@ Copyright (c) 2019 Travis Bemann
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

	.include "src/common/vectors.s"
	.word handle_null+1 @ 16: window watchdog
	.word handle_null+1 @ 17: PVD
	.word handle_null+1 @ 18: tamper and timestamp
	.word handle_null+1 @ 19: RTC
	.word handle_null+1 @ 20: flash
	.word handle_null+1 @ 21: RCC
	.word handle_exti_0+1 @ 22: EXTI 0
	.word handle_exti_1+1 @ 23: EXTI 1
	.word handle_exti_2+1 @ 24: EXTI 2
	.word handle_exti_3+1 @ 25: EXTI 3
	.word handle_exti_4+1 @ 26: EXTI 4
	.word handle_null+1 @ 27: DMA1 0
	.word handle_null+1 @ 28: DMA1 1
	.word handle_null+1 @ 29: DMA1 2
	.word handle_null+1 @ 30: DMA1 3
	.word handle_null+1 @ 31: DMA1 4
	.word handle_null+1 @ 32: DMA1 5
	.word handle_null+1 @ 33: DMA1 6
	.word handle_adc+1 @ 34: ADC
	.word handle_null+1 @ 35:
	.word handle_null+1 @ 36:
	.word handle_null+1 @ 37:
	.word handle_null+1 @ 38:
	.word handle_exti_9_5+1 @ 39: EXTI9_5
	.word handle_null+1 @ 40:
	.word handle_null+1 @ 41:
	.word handle_null+1 @ 42:
	.word handle_null+1 @ 43:
	.word handle_time_2+1 @ 44: timer 2
	.word handle_time_3+1 @ 45: timer 3
	.word handle_time_4+1 @ 46: timer 4
	.word handle_null+1 @ 47:
	.word handle_null+1 @ 48:
	.word handle_null+1 @ 49:
	.word handle_null+1 @ 50:
	.word handle_null+1 @ 51:
	.word handle_null+1 @ 52:
	.word handle_null+1 @ 53:
	.word handle_null+1 @ 54:
	.word handle_null+1 @ 55:
	.word handle_exti_15_10+1 @ 56: EXTI15_10
	.word handle_null+1 @ 57:
	.word handle_null+1 @ 58:
	.word handle_null+1 @ 59:
	.word handle_null+1 @ 60:
	.word handle_null+1 @ 61:
	.word handle_null+1 @ 62:
	.word handle_null+1 @ 63:
	.word handle_null+1 @ 64:
	.word handle_null+1 @ 65:
	.word handle_null+1 @ 66:
	.word handle_null+1 @ 67:
	.word handle_null+1 @ 68:
	.word handle_null+1 @ 69:
	.word handle_null+1 @ 70:
	.word handle_null+1 @ 71:
	.word handle_null+1 @ 72:
	.word handle_null+1 @ 73:
	.word handle_null+1 @ 74:
	.word handle_null+1 @ 75:
	.word handle_null+1 @ 76:
	.word handle_null+1 @ 77:
	.word handle_null+1 @ 78:
	.word handle_null+1 @ 79:
	.word handle_null+1 @ 80:
	.word handle_null+1 @ 81:
	.word handle_null+1 @ 97:
	.word handle_null+1 @ 98:
	.word handle_null+1 @ 99:
	.word handle_null+1 @ 100:
	.word handle_null+1 @ 101:
	.word handle_null+1 @ 102:
	.word handle_null+1 @ 103:
	.word handle_null+1 @ 104:
	.word handle_null+1 @ 105:
	.word handle_null+1 @ 106:
	.word handle_null+1 @ 107:
	.word handle_null+1 @ 108:
	.word handle_null+1 @ 109:
	.word handle_null+1 @ 110:
	.word handle_null+1 @ 111:
	.word handle_null+1 @ 112:
	.word handle_null+1 @ 113:
	.word handle_null+1 @ 114:
	.word handle_null+1 @ 115:
	.word handle_null+1 @ 116:
	.word handle_null+1 @ 117:
	.word handle_null+1 @ 118:
	.word handle_null+1 @ 119:
	.word handle_null+1 @ 120:
	
