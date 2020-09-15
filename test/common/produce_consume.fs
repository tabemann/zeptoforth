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

\ Set up the wordlist order
forth-wordlist task-wordlist chan-wordlist 3 set-order
forth-wordlist set-current

\ My channel size
16 constant my-chan-size

\ My channel
my-chan-size chan-size buffer: my-chan

\ Initialize my channel
my-chan my-chan-size init-chan

\ My producer
: producer ( -- )
  begin
    [char] Z 1+ [char] A ?do
      i my-chan send-chan-byte
    loop
  again
;

\ My consumer
: consumer ( -- )
  begin
    my-chan recv-chan-byte emit
  again
;

\ My producer task
variable producer-task

\ My consumer task
variable consumer-task

\ Spawn my producer task
' producer 256 256 256 spawn producer-task !

\ Spawn my consumer task
' consumer 256 256 256 spawn consumer-task !

\ Enable my producer task
producer-task @ enable-task

\ Enable my consumer task
consumer-task @ enable-task

\ Initiate execution
pause