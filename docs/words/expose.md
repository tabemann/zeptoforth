# Exposed Kernel Variables

Thes words are in `forth-wordlist`.

##### `state`

Get the STATE variable address

##### `base`

Get the BASE variable address

##### `pause-enabled`

Get the PAUSE enabled variable address

##### `dict-base`

Get the RAM dictionary base variable address
	
##### `ram-base`

Get the RAM base

##### `ram-end`

Get the RAM end

##### `flash-base`

Get the flash base

##### `flash-end`

Get the flash end
	
##### `stack-base`

Get the current stack base variable address

##### `stack-end`

Get the current stack end variable address

##### `rstack-base`

Get the current return stack base variable address

##### `rstack-end`

Get the current returns stack end variable address

##### `handler`

Get the current exception handler variable address

##### `>parse`

The parse index

##### `source`

The source info

##### `build-target`

Get the address to store a literal in for the word currently being
built

##### `sys-ram-dict-base`

Get the base of the system RAM dictionary space

##### `>in`

The input buffer index

##### `input#`

The input buffer count

##### `input`

The input buffer

##### `order-count`

The wordlist count

##### `order`

The wordlist order
	
##### `prompt-hook`

The prompt hook

##### `handle-number-hook`

The handle number hook

##### `failed-parse-hook`

The failed parse hook

##### `emit-hook`

The emit hook

##### `emit?-hook`

The emit? hook

##### `key-hook`

The key hook

##### `key?-hook`

The key? hook

##### `refill-hook`

The refill hook

##### `pause-hook`

The pause hook

##### `validate-dict-hook`

The dictionary size validation hook

##### `fault-handler-hook`

Get the FAULT-HANDLER-HOOK variable address

##### `null-handler-hook`

Get the NULL-HANDLER-HOOK variable address

##### `svcall-handler-hook`

Get the SVCALL-HANDLER-HOOK variable address

##### `pendsv-handler-hook`

Get the PENDSV-HANDLER-HOOK variable address

##### `systick-handler-hook`

Get the SYSTICK-HANDLER-HOOK variable address
	
