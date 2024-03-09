#!/bin/sh

# Copyright (c) 2020-2024 Travis Bemann
# Copyright (c) 2023 Chris Salch
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


# This script should only be sourced.
# See: https://superuser.com/a/731431
if [ "$(basename -- "$0")" = "common.sh" ]; then
    >&2 echo "Don't run $0, source it"
    exit 1
fi

######
# Check for a useable screen
#

# Make sure there is not screen session running
cleanup() {
  screen -X quit || true
}

# macOs screen will send the \n as \n instead of a newline.
check_screen() {
  if [ "$(uname -s)" = "Darwin" ] && (screen -v | grep '4.00.* (FAU)' >/dev/null); then
    cat 2>&1 <<EOD
The version of screen pacakged with macOs is known to not send newlines
correctly when using stuff. Please install GNU screen via Hombrew.

    brew install screen
EOD
    exit 1
  fi

  trap cleanup EXIT
}


######
# Virtual Environment handling.
#

# Name of the virtual environment directory to use.
ZEPTOFORTH_VENV="${DIR}/zeptoforth_venv"

# Make sure the virtual environment is setup.
make_venv() {
  (
    cd ${DIR}

    if [ ! -d "${ZEPTOFORTH_VENV}" ] || [ ! -e "${ZEPTOFORTH_VENV}/bin/activate" ]; then
      echo "Creating zeptoforth virtual environment."
      python3 -m venv "${ZEPTOFORTH_VENV}"
      . "${ZEPTOFORTH_VENV}/bin/activate"
      pip install -r ./requirements.txt
    fi 
  )
}

# Make sure we have a virtual environment
activate_venv() {

  if ! make_venv; then
    cat 2>&1 <<EOD
  Something went wrong creating codeloader3 virtualenvironment.
  Please remove the old virtual environment, correct any reported
  problems and try again.

  To remove the broken virtual environment use:

      rm -r ${ZEPTOFORTH_VENV}
EOD
    exit 1  
  fi

  # Activate the virtualenvironment
  . "${ZEPTOFORTH_VENV}/bin/activate"
}

######
# IHEX functions
#

codeloader() ( 
  PORT=$1
  SRC=$2

  ${DIR}/codeload3.sh -B 115200 -p ${PORT} serial ${SRC}
)

screen_download() (
  PORT=$1
  SRC=$2
  TARGET=$3

  codeloader ${PORT} ${SRC}
  
  rm screenlog.0 || true
  screen -d -m ${PORT} 115200
  screen -X logtstamp off
  screen -X log on
  screen -X stuff 'clone\n'
  until grep 'clone_end' screenlog.0
  do
      sleep 1
  done
  screen -X log off
  screen -X quit
  sleep 1
  sed '1d' screenlog.0 > inter
  sed '$d' inter > inter.1
  sed '$d' inter.1 > inter
  sed 's/:00000001FF clone_end/:00000001FF/' inter > inter.1
  mv inter.1 ${TARGET}.ihex
  arm-none-eabi-objcopy -I ihex -O binary ${TARGET}.ihex ${TARGET}.bin
  rm screenlog.0
  rm inter
)

screen_download_ihex() ( 
  PORT=$1
  TARGET=$2

  screen_download ${PORT} src/common/forth/ihex.fs ${TARGET}
)


screen_download_ihex_minidict() (
  PORT=$1
  TARGET=$2

  screen_download ${PORT} src/common/forth/ihex_minidict.fs ${TARGET}
)

flash_rp2040() {
    IMAGE=$1
    BLOCK_DEVICE=$2
    FILESYSTEM=$3
    udisksctl mount -b ${BLOCK_DEVICE}
    sleep 1
    cp ${IMAGE} ${FILESYSTEM}
    sleep 8
}

issue_bootsel() {
    PORT=$1
    screen -d -m ${PORT} 115200
    screen -X stuff 'bootsel\n'
    screen -X quit
    sleep 5
}
