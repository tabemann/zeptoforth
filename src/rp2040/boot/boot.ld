MEMORY
{
	ram : org = 0x20041f00, len = 252
}

SECTIONS
{
	.text : { *(.text*) } > ram
}
