#!/bin/sh

# Copyright (c) 2020 Travis Bemann
#
# Permission is hereby granted, free of charge, to any person obtaining a copy
# of this software and associated documentation files (the "Software"), to deal
# in the Software without restriction, including without limitation the rights
# to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
# copies of the Software, and to permit persons to whom the Software is
# furnished to do so, subject to the following conditions:
# 
# The above copyright notice and this permission notice shall be included in
# all copies or substantial portions of the Software.
# 
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
# IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
# AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
# LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
# OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
# SOFTWARE.

VERSION=$1

mkdir ../zeptoforth-$VERSION
rsync -aP --exclude=".git" --exclude="bin" ./ ../zeptoforth-$VERSION/
mkdir ../zeptoforth-$VERSION/bin
cp -r bin/$VERSION ../zeptoforth-$VERSION/bin
cd ../zeptoforth-$VERSION
rm -rf `find . -name 'screenlog.*'`
make clean
rm -rf `find . -name '*~'`
rm -rf obj
rm -rf upload.fs
make html
make epub
rm -rf docs
mv html docs
rm -rf utils/zeptoforth_venv
cd ..
tar cfz zeptoforth-$VERSION.tar.gz zeptoforth-$VERSION
