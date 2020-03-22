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
	.word handle_null+1 @ 39:
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
	.word handle_null+1 @ 56:
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
	
