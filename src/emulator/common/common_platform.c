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

/* Reset register */
#define REG_RESET 0xE000ED0C

/* Reset key value */
#define RESET_KEY 0x05FA0004

/* Load common peripheral */
uint32_t load_common_peripheral(uint32_t address, int bits) {
}

/* Store common peripheral */
void store_common_peripheral(uint32_t address, uint32_t value, int bits) {
  if(address == REG_RESET) {
    if(bits == 32) {
      if(value == RESET_KEY) {
	reset();
      }
    }
  }
}

/* Common reset */
void common_reset(void) {
  state.registers[PC] = load_32(0x00000000) & ~0x1;
}

