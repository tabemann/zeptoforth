\ Copyright (c) 2021-2024 Travis Bemann
\
\ Permission is hereby granted, free of charge, to any person obtaining a copy
\ of this software and associated documentation files (the "Software"), to deal
\ in the Software without restriction, including without limitation the rights
\ to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
\ copies of the Software, and to permit persons to whom the Software is
\ furnished to do so, subject to the following conditions:
\ 
\ The above copyright notice and this permission notice shall be included in
\ all copies or substantial portions of the Software.
\ 
\ THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
\ IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
\ FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
\ AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
\ LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
\ OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
\ SOFTWARE.

\ This is not actual Forth code, but rather setup directives for e4thcom to be
\ executed to load a "big" configuration

#include src/common/forth/compat.fs
#include src/common/forth/implicit.fs
#include src/common/forth/dynamic.fs
#include src/common/forth/closure.fs
#include src/common/forth/temp.fs
#include src/common/forth/temp_str.fs
#include src/common/forth/disassemble.fs
#include src/common/forth/pool.fs
#include src/common/forth/heap.fs
#include src/common/forth/map.fs
#include src/common/forth/cstr_map.fs
#include src/common/forth/int_map.fs
#include src/common/forth/oo.fs
#include src/common/forth/slock.fs
#include src/common/forth/core_lock.fs
#include src/common/forth/action.fs
#include src/common/forth/tqueue.fs
#include src/common/forth/lock.fs
#include src/common/forth/int_overload.fs
#include src/common/forth/alarm.fs
#include src/common/forth/semaphore.fs
#include src/common/forth/fchannel.fs
#include src/common/forth/channel.fs
#include src/common/forth/schannel.fs
#include src/common/forth/rchannel.fs
#include src/common/forth/stream.fs
#include src/common/forth/console.fs
#include src/common/forth/ansi_term.fs
#include src/common/forth/line.fs
#include src/common/forth/more_words.fs
#include src/common/forth/more.fs
#include src/common/forth/task_pool.fs
#include src/common/forth/action_pool.fs
#include src/common/forth/tinymt.fs
#include src/common/forth/monitor.fs
