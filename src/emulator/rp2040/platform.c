/* Copyright (c) 2022 Travis Bemann
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE. */

#include <stdlib.h>
#include <stdio.h>
#include <stdint.h>

#include "emulator.h"

/* SIO base address */
#define SIO_BASE 0xD0000000

/* SIO divider registers */
#define SIO_DIV_UDIVIDEND (SIO_BASE + 0x60)
#define SIO_DIV_UDIVISOR (SIO_BASE + 0x64)
#define SIO_DIV_SDIVIDEND (SIO_BASE + 0x68)
#define SIO_DIV_SDIVISOR (SIO_BASE + 0x6C)
#define SIO_DIV_QUOTIENT (SIO_BASE + 0x70)
#define SIO_DIV_REMAINDER (SIO_BASE + 0x74)
#define SIO_DIV_CSR (SIO_BASE + 0x78)

#define SIO_DIV_CSR_DIRTY 0x2

/* SIO divider register index */
#define SIO_DIV(reg) (((reg) - SIO_DIV_UDIVIDEND) >> 2)

/* SIO divider structure */
typedef struct divider_t {
  uint32_t regs[7];
} divider_t;

divider_t divider;

/* Divider register */
#define DIVIDER_REG(reg) \
  (divider.regs[(((reg) - SIO_DIV_UDIVIDEND) >> 2)])

/* Truncate a value to a number of bits */
#define TRUNCATE(value, bits) ((value) & (0xFFFFFFFF >> (32 - bits)))

/* Load peripheral */
uint32_t load_peripheral(uint32_t address, int bits) {
  if((address >= SIO_DIV_UDIVIDEND) && (address <= SIO_DIV_CSR)) {
    if((address & 0x3) == 0) {
      if(address == SIO_DIV_QUOTIENT) {
	DIVIDER_REG(SIO_DIV_CSR) &= ~SIO_DIV_CSR_DIRTY;
      }
      return TRUNCATE(DIVIDER_REG(address), bits);
    } else {
      return 0;
    }
  }
  return load_common_peripheral(address, bits);
}

/* Store peripheral */
void store_peripheral(uint32_t address, uint32_t value, int bits) {
  if((address >= SIO_DIV_UDIVIDEND) && (address <= SIO_DIV_REMAINDER)) {
    if((address & 0x3) == 0) {
      DIVIDER_REG(address) = value;
      DIVIDER_REG(SIO_DIV_CSR) |= SIO_DIV_CSR_DIRTY;
      if(address == SIO_DIV_UDIVISOR) {
	if(DIVIDER_REG(SIO_DIV_UDIVISOR) != 0) {
	  DIVIDER_REG(SIO_DIV_QUOTIENT) =
	    DIVIDER_REG(SIO_DIV_UDIVIDEND) / DIVIDER_REG(SIO_DIV_UDIVISOR);
	  DIVIDER_REG(SIO_DIV_REMAINDER) =
	    DIVIDER_REG(SIO_DIV_UDIVIDEND) % DIVIDER_REG(SIO_DIV_UDIVISOR);
	}
      } else if(address == SIO_DIV_SDIVISOR) {
	if(DIVIDER_REG(SIO_DIV_SDIVISOR) != 0) {
	  DIVIDER_REG(SIO_DIV_QUOTIENT) =
	    (uint32_t)((int32_t)DIVIDER_REG(SIO_DIV_SDIVIDEND) /
		       (int32_t)DIVIDER_REG(SIO_DIV_SDIVISOR));
	  DIVIDER_REG(SIO_DIV_REMAINDER) =
	    (uint32_t)((int32_t)DIVIDER_REG(SIO_DIV_SDIVIDEND) %
		       (int32_t)DIVIDER_REG(SIO_DIV_SDIVISOR));
	}
      }
    }
    return;
  }
  store_common_peripheral(address, value, bits);
}

/* Reset */
void reset(void) {
  for(int i = 0; i < 7; i++) {
    divider.regs[i] = 0;
  }
  common_reset();
}
