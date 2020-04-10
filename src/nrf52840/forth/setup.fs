\ Copyright (c) 2020 Travis Bemann
\
\ This program is free software: you can redistribute it and/or modify
\ it under the terms of the GNU General Public License as published by
\ the Free Software Foundation, either version 3 of the License, or
\ (at your option) any later version.
\
\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.
\
\ You should have received a copy of the GNU General Public License
\ along with this program.  If not, see <http://www.gnu.org/licenses/>.

\ This is not actual Forth code, but rather setup directives for e4thcom to be
\ executed from the root of the zeptoforth directory to initialize zeptoforth
\ on an STM32L476 device.

#include src/common/forth/basic.fs
#include src/stm32l476/forth/erase.fs
#include src/common/forth/lambda.fs
#include src/common/forth/systick.fs
#include src/stm32l476/forth/int_io.fs
#include src/common/forth/task.fs
#include src/common/forth/schedule.fs

\ Set a cornerstone to enable deleting everything compiled after this code
cornerstone restore-state

