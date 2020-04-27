#!/bin/sh

# Copyright (c) 2020 Travis Bemann
#
# This program is free software: you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation, either version 3 of the License, or
# (at your option) any later version.
#
# This program is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU General Public License for more details.
#
# You should have received a copy of the GNU General Public License
# along with this program.  If not, see <http://www.gnu.org/licenses/>.

MARKDOWN=~/Markdown_1.0.1/Markdown.pl
DEST_DIR=html

mkdir -p $DEST_DIR

for file in docs/*.md
do
    base=$(basename -- "$file" | cut -f 1 -d '.')
    $MARKDOWN < "$file" > "$DEST_DIR/$base.html"
done
