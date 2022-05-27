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

#ifndef __EMULATOR_H__
#define __EMULATOR_H__

#include <stdint.h>

/* The PC register */
#define PC 15

/* The LR register */
#define LR 14

/* The SP register */
#define SP 13

/* The condition bits in the APSR */
#define APSR_N 0x80000000
#define APSR_Z 0x40000000
#define APSR_C 0x20000000
#define APSR_V 0x10000000

/* The VTOR field */
#define VTOR 0xE000ED08

/* Platform-specific settings */

#ifdef STM32F407
#define RAM_START 0x20000000
#define RAM_END 0x20020000
#define FLASH_START 0x00000000
#define FLASH_END 0x00100000
#endif /* STM32F407 */

#ifdef STM32L476
#define RAM_START 0x20000000
#define RAM_END 0x20018000
#define FLASH_START 0x00000000
#define FLASH_END 0x00100000
#endif /* STM32L476 */

#ifdef STM32F746
#define RAM_START 0x20000000
#define RAM_END 0x20050000
#define FLASH_START 0x00200000
#define FLASH_END 0x00300000
#endif /* STM32F746 */

#ifdef RP2040
#define RAM_START 0x20000000
#define RAM_END 0x20042000
#define FLASH_START 0x10000000
#define FLASH_END 0x10200000
#define AUX_FLASH_START 0x00000000
#define AUX_FLASH_END 0x00004000
#endif /* RP2040 */

/* The 16-bit instruction handler */
typedef void (*handler_t)(uint16_t instr);

/* The global state structure */
typedef struct state_t {
  uint8_t* ram;
  uint8_t* flash;
#ifdef RP2040
  uint8_t* aux_flash;
#endif
  uint32_t registers[16];
  handler_t handlers[65536];
  uint32_t apsr;
} state_t;

/* Register a handler */
void register_handler(handler_t handler, uint16_t mask, uint16_t key);

/* Default handler */
void default_handler(uint16_t instr);

/* Load byte */
uint8_t load_8(uint32_t address);

/* Load halfword */
uint16_t load_16(uint32_t address);

/* Load word */
uint32_t load_32(uint32_t address);

/* Load peripheral */
uint32_t load_peripheral(uint32_t address, int bits);

/* Load common peripheral */
uint32_t load_common_peripheral(uint32_t address, int bits);

/* Store byte */
void store_8(uint32_t address, uint8_t value);

/* Store halfword */
void store_16(uint32_t address, uint16_t value);

/* Store word */
void store_32(uint32_t address, uint32_t value);

/* Store peripheral */
void store_peripheral(uint32_t address, uint32_t value, int bits);

/* Store common peripheral */
void store_common_peripheral(uint32_t address, uint32_t value, int bits);

/* Reset */
void reset(void);

/* Common reset */
void common_reset(void);

/* Load an image into flash */
void load_image(int argc, char** argv);

#ifdef RP2040
/* Load an image into auxiliary flash */
void load_aux_image(int argc, char** argv);
#endif

/* Execute insttructions */
void execute(void);

/* Register instruction set */
void register_instr_set(void);

#endif /* __EMULATOR_H__ */
