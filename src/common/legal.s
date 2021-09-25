@ Copyright (c) 2019-2021 Travis Bemann
@
@ Permission is hereby granted, free of charge, to any person obtaining a copy
@ of this software and associated documentation files (the "Software"), to deal
@ in the Software without restriction, including without limitation the rights
@ to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
@ copies of the Software, and to permit persons to whom the Software is
@ furnished to do so, subject to the following conditions:
@ 
@ The above copyright notice and this permission notice shall be included in
@ all copies or substantial portions of the Software.
@ 
@ THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
@ IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
@ FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
@ AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
@ LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
@ OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
@ SOFTWARE.

	@@ Display the license notice
	define_word "license", visible_flag
_license:
	push {lr}
	bl _cr
	string_ln "Copyright (c) 2019-2021 Travis Bemann"
	bl _type
	string_ln "Copyright (c) 2013, 2021 Matthias Koch"
	bl _type
	string_ln "Copyright (c) 2021 Jan Bramkamp"
	bl _type
	bl _cr
	string_ln "Permission is hereby granted, free of charge, to any person obtaining a copy"
	bl _type
	string_ln "of this software and associated documentation files (the \"Software\"), to deal"
	bl _type
	string_ln "in the Software without restriction, including without limitation the rights"
	bl _type
	string_ln "to use, copy, modify, merge, publish, distribute, sublicense, and/or sell"
	bl _type
	string_ln "copies of the Software, and to permit persons to whom the Software is"
	bl _type
	string_ln "furnished to do so, subject to the following conditions:"
	bl _type
	bl _cr
	string_ln "The above copyright notice and this permission notice shall be included in"
	bl _type
	string_ln "all copies or substantial portions of the Software."
	bl _type
	bl _cr
	string_ln "THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR"
	bl _type
	string_ln "IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,"
	bl _type
	string_ln "FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE"
	bl _type
	string_ln "AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER"
	bl _type
	string_ln "LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,"
	bl _type
	string_ln "OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE"
	bl _type
	string_ln "SOFTWARE."
	bl _type

	pop {pc}

	.ltorg
