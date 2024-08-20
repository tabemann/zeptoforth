MEMORY
{
	flash : org = 0x10000000, len = 4096
}

SECTIONS
{
	. = ORIGIN(flash);
	.text : { *(.text*) } > flash
}
