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
#include <errno.h>

#include "emulator.h"

state_t state;

/* Register a handler */
void register_handler(handler_t handler, uint16_t mask, uint16_t key) {
  for(int i = 0; i < 65536; i++) {
    uint16_t instr = ((uint16_t)i & ~mask) | (key & mask);
    if(state.handlers[instr] != default_handler &&
       state.handlers[instr] != handler) {
      fprintf(stderr, "OVERLAPPING HANDLERS: mask: %04X key: %04X i: %04X\n",
	      (uint32_t)mask, (uint32_t)key, (uint32_t)i);
      exit(1);
    }
    state.handlers[instr] = handler;
  }
}

/* Default handler */
void default_handler(uint16_t instr) {
  fprintf(stderr, "UNRECOGNIZED INSTRUCTION: %04X @ %08X\n",
	  (uint32_t)instr, state.registers[15]);
  exit(1);
}

/* Unaligned access */
void unaligned_access(uint32_t address) {
  fprintf(stderr, "UNALIGNED ACCESS: %08X\n", address);
  exit(1);
}

/* Load byte */
uint8_t load_8(uint32_t address) {
  if(address >= RAM_START && address < RAM_END) {
    uint32_t offset = address - RAM_START;
    return state.ram[offset];
  }
  if(address >= FLASH_START && address < FLASH_END) {
    uint32_t offset = address - FLASH_START;
    return state.flash[offset];
  }
#ifdef RP2040
  if(address >= AUX_FLASH_START && address < AUX_FLASH_END) {
    uint32_t offset = address - AUX_FLASH_START;
    return state.aux_flash[offset];
  }
#endif
  return (uint8_t)(load_peripheral(address, 8) & 0xFF);
}

/* Load halfword */
uint16_t load_16(uint32_t address) {
#ifdef RP2040
  if(address & 0x1) {
    unaligned_access(address);
  }
#endif
#ifndef LITTLE_ENDIAN
  if(address >= RAM_START && address < RAM_END) {
    uint32_t offset = address - RAM_START;
    return (uint16_t)state.ram[offset] |
      ((uint16_t)state.ram[offset + 1] << 8)
  }
  if(address >= FLASH_START && address < FLASH_END) {
    uint32_t offset = address - FLASH_START;
    return (uint16_t)state.flash[offset] |
      ((uint16_t)state.flash[offset + 1] << 8);
  }
#ifdef RP2040
  if(address >= AUX_FLASH_START && address < AUX_FLASH_END) {
    uint32_t offset = address - AUX_FLASH_START;
    return (uint16_t)state.aux_flash[offset] |
      ((uint16_t)state.aux_flash[offset + 1] << 8);
  }
#endif
#else
  if(address >= RAM_START && address < RAM_END) {
    uint32_t offset = address - RAM_START;
    return *(uint16_t*)(&state.ram[offset]);
  }
  if(address >= FLASH_START && address < FLASH_END) {
    uint32_t offset = address - FLASH_START;
    return *(uint16_t*)(&state.flash[offset]);
  }
#ifdef RP2040
  if(address >= AUX_FLASH_START && address < AUX_FLASH_END) {
    uint32_t offset = address - AUX_FLASH_START;
    return *(uint16_t*)(&state.aux_flash[offset]);
  }
#endif
#endif
  return (uint16_t)(load_peripheral(address, 16) & 0xFFFF);
}

/* Load word */
uint32_t load_32(uint32_t address) {
#ifdef RP2040
  if(address & 0x3) {
    unaligned_access(address);
  }
#endif
#ifndef LITTLE_ENDIAN
  if(address >= RAM_START && address < RAM_END) {
    uint32_t offset = address - RAM_START;
    return (uint32_t)state.ram[offset] |
      ((uint32_t)state.ram[offset + 1] << 8) |
      ((uint32_t)state.ram[offset + 2] << 16) |
      ((uint32_t)state.ram[offset + 3] << 24);
  }
  if(address >= FLASH_START && address < FLASH_END) {
    uint32_t offset = address - FLASH_START;
    return (uint32_t)state.flash[offset] |
      ((uint32_t)state.flash[offset + 1] << 8) |
      ((uint32_t)state.flash[offset + 2] << 16) |
      ((uint32_t)state.flash[offset + 3] << 24);
  }
#ifdef RP2040
  if(address >= AUX_FLASH_START && address < AUX_FLASH_END) {
    uint32_t offset = address - AUX_FLASH_START;
    return (uint32_t)state.aux_flash[offset] |
      ((uint32_t)state.aux_flash[offset + 1] << 8) |
      ((uint32_t)state.aux_flash[offset + 2] << 16) |
      ((uint32_t)state.aux_flash[offset + 3] << 24);
  }
#endif
#else
  if(address >= RAM_START && address < RAM_END) {
    uint32_t offset = address - RAM_START;
    return *(uint32_t*)(&state.ram[offset]);
  }
  if(address >= FLASH_START && address < FLASH_END) {
    uint32_t offset = address - FLASH_START;
    return *(uint32_t*)(&state.flash[offset]);
  }
#ifdef RP2040
  if(address >= AUX_FLASH_START && address < AUX_FLASH_END) {
    uint32_t offset = address - AUX_FLASH_START;
    return *(uint32_t*)(&state.aux_flash[offset]);
  }
#endif
#endif
  return (uint32_t)load_peripheral(address, 16);
}

/* Store byte */
void store_8(uint32_t address, uint8_t value) {
  if(address >= RAM_START && address < RAM_END) {
    uint32_t offset = address - RAM_START;
    state.ram[offset] = value;
    return;
  }
  if(address >= FLASH_START && address < FLASH_END) {
    uint32_t offset = address - FLASH_START;
    state.flash[offset] = value;
    return;
  }
#ifdef RP2040
  if(address >= AUX_FLASH_START && address < AUX_FLASH_END) {
    uint32_t offset = address - AUX_FLASH_START;
    state.aux_flash[offset] = value;
    return;
  }
#endif
  store_peripheral(address, (uint32_t)value, 8);
}

/* Store halfword */
void store_16(uint32_t address, uint16_t value) {
#ifdef RP2040
  if(address & 0x1) {
    unaligned_access(address);
  }
#endif
#ifndef LITTLE_ENDIAN
  if(address >= RAM_START && address < RAM_END) {
    uint32_t offset = address - RAM_START;
    state.ram[offset] = (uint8_t)(value & 0xFF);
    state.ram[offset + 1] = (uint8_t)((value >> 8) & 0xFF);
    return;
  }
  if(address >= FLASH_START && address < FLASH_END) {
    uint32_t offset = address - FLASH_START;
    state.flash[offset] = (uint8_t)(value & 0xFF);
    state.flash[offset + 1] = (uint8_t)((value >> 8) & 0xFF);
    return;
  }
#ifdef RP2040
  if(address >= AUX_FLASH_START && address < AUX_FLASH_END) {
    uint32_t offset = address - AUX_FLASH_START;
    state.aux_flash[offset] = (uint8_t)(value & 0xFF);
    state.aux_flash[offset + 1] = (uint8_t)((value >> 8) & 0xFF);
    return;
  }
#endif
#else
  if(address >= RAM_START && address < RAM_END) {
    uint32_t offset = address - RAM_START;
    *(uint16_t*)(&state.ram[offset]) = value & 0xFFFF;
    return;
  }
  if(address >= FLASH_START && address < FLASH_END) {
    uint32_t offset = address - FLASH_START;
    *(uint16_t*)(&state.flash[offset]) = value & 0xFFFF;
    return;
  }
#ifdef RP2040
  if(address >= AUX_FLASH_START && address < AUX_FLASH_END) {
    uint32_t offset = address - AUX_FLASH_START;
    *(uint16_t*)(&state.aux_flash[offset]) = value & 0xFFFF;
    return;
  }
#endif  
#endif
  store_peripheral(address, (uint32_t)value, 16);
}

/* Store word */
void store_32(uint32_t address, uint32_t value) {
#ifdef RP2040
  if(address & 0x3) {
    unaligned_access(address);
  }
#endif
#ifndef LITTLE_ENDIAN
  if(address >= RAM_START && address < RAM_END) {
    uint32_t offset = address - RAM_START;
    state.ram[offset] = (uint8_t)(value & 0xFF);
    state.ram[offset + 1] = (uint8_t)((value >> 8) & 0xFF);
    state.ram[offset + 2] = (uint8_t)((value >> 16) & 0xFF);
    state.ram[offset + 3] = (uint8_t)((value >> 24) & 0xFF);
    return;
  }
  if(address >= FLASH_START && address < FLASH_END) {
    uint32_t offset = address - FLASH_START;
    state.flash[offset] = (uint8_t)(value & 0xFF);
    state.flash[offset + 1] = (uint8_t)((value >> 8) & 0xFF);
    state.flash[offset + 2] = (uint8_t)((value >> 16) & 0xFF);
    state.flash[offset + 3] = (uint8_t)((value >> 24) & 0xFF);
    return;
  }
#ifdef RP2040
  if(address >= AUX_FLASH_START && address < AUX_FLASH_END) {
    uint32_t offset = address - AUX_FLASH_START;
    state.aux_flash[offset] = (uint8_t)(value & 0xFF);
    state.aux_flash[offset + 1] = (uint8_t)((value >> 8) & 0xFF);
    state.aux_flash[offset + 2] = (uint8_t)((value >> 16) & 0xFF);
    state.aux_flash[offset + 3] = (uint8_t)((value >> 24) & 0xFF);
    return;
  }
#endif
#else
  if(address >= RAM_START && address < RAM_END) {
    uint32_t offset = address - RAM_START;
    *(uint32_t*)(&state.ram[offset]) = value;
    return;
  }
  if(address >= FLASH_START && address < FLASH_END) {
    uint32_t offset = address - FLASH_START;
    *(uint32_t*)(&state.flash[offset]) = value;
    return;
  }
#ifdef RP2040
  if(address >= AUX_FLASH_START && address < AUX_FLASH_END) {
    uint32_t offset = address - AUX_FLASH_START;
    *(uint32_t*)(&state.aux_flash[offset]) = value;
    return;
  }
#endif
#endif
  store_peripheral(address, value, 8);
}

/* Load an image into flash */
void load_image(int argc, char** argv) {
  if(argc < 2) {
    fprintf(stderr, "Missing image argument\n");
    exit(1);
  }
  FILE* file = fopen(argv[1], "rb");
  if(!file) {
    perror(argv[1]);
    exit(1);
  }
  size_t size = 0;
  size_t total_size = 0;
  do {
    size = fread((void*)(state.flash + total_size), 1
		 (FLASH_END - FLASH_START) - total_size, file);
    total_size += size;
  } while(total_size < (FLASH_END - FLASH_START) &&
	  !feof(file) && !ferror(file));
  if(ferror(file)) {
    perror(argv[1]);
    exit(1);
  }
  fclose(file);
}

/* Load an image into auxiliary flash */
void load_aux_image(int argc, char** argv) {
  if(argc < 3) {
    fprintf(stderr, "Missing auxiliary image argument\n");
    exit(1);
  }
  FILE* file = fopen(argv[2], "rb");
  if(!file) {
    perror(argv[1]);
    exit(1);
  }
  size_t size = 0;
  size_t total_size = 0;
  do {
    size = fread((void*)(state.aux_flash + total_size), 1
		 (AUX_FLASH_END - AUX_FLASH_START) - total_size, file);
    total_size += size;
  } while(total_size < (AUX_FLASH_END - AUX_FLASH_START) &&
	  !feof(file) && !ferror(file));
  if(ferror(file)) {
    perror(argv[2]);
    exit(1);
  }
  fclose(file);
}

/* Execute instructions */
void execute(void) {
  while(1) {
    uint16_t instr = load_16(state.registers[PC]);
    state.handlers[instr](instr);
  }
}

/* The entry point to the emulator */
int main(int argc, char** argv) {
  state.ram = malloc((RAM_END - RAM_START) + 4);
  state.flash = malloc((FLASH_END - FLASH_START) + 4);
#ifdef RP2040
  state.aux_flash = malloc((AUX_FLASH_END - AUX_FLASH_START) + 4);
#endif
  for(uint64_t i = 0; i < (RAM_END - RAM_START) + 4; i++) {
    state.ram[i] = 0x00;
  }
  for(uint64_t i = 0; i < (FLASH_END - FLASH_START) + 4; i++) {
    state.flash[i] = 0xFF;
  }
#ifdef RP2040
  for(uint64_t i = 0; i < (AUX_FLASH_END - AUX_FLASH_START) + 4; i++) {
    state.aux_flash[i] = 0xFF;
  }
#endif
  for(uint64_t i = 0; i < 15; i++) {
    state.registers[i] = 0x00000000;
  }
  state.status = 0x00000000;
  for(int i = 0; i < 65536; i++) {
    state.handlers[i] = default_handler;
  }
  load_image(argc, argv);
#ifdef RP2040
  load_aux_image(argc, argv);
#endif
  state.registers[PC] = load_32(0x00000000) & ~0x1;
  execute();
  return 0;
}
