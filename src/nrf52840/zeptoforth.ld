MEMORY
{
	flash : org = 0x00000000, len = 1024k
	ram : org = 0x20000000, len = 256k
}

SECTIONS
{
	.text : { *(.text*) } > flash
	.bss : { *(.bss*) } > ram
}
