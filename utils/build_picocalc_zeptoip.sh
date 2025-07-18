#!/bin/sh
set -e

# Copyright (c) 2025 Travis Bemann
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

usage() {
    echo "Usage: $0 <platform> <port> (ipv4 | ipv6) <fw> <fw_clm> (5x8 | 6x8 | 7x8) [graphical | text] [not_pico_plus | pico_plus] [core_0 | core_1]"
}

if [ "$#" -lt 6 ]; then
    usage
    exit 1
fi
PLATFORM="$1"
PORT="$2"
if [ "$3" = 'ipv4' ]; then
    ZEPTOIP='ipv4'
elif [ "$3" = 'ipv6' ]; then
    ZEPTOIP='ipv6'
else
    usage
    exit 1
fi
FW="$4"
FW_CLM="$5"
if [ "$6" = '5x8' ]; then
    FONT='5x8'
elif [ "$6" = '6x8' ]; then
    FONT='6x8'
elif [ "$6" = '7x8' ]; then
    FONT='7x8'
else
    usage
    exit 1
fi
if [ "$#" -ge 7 ]; then
    if [ "$7" = 'text' ]; then
        TEXT_ONLY='text'
    elif [ "$7" = 'graphical' ]; then
        TEXT_ONLY='graphical'
    else
        usage
        exit 1
    fi
else
    TEXT_ONLY='graphical'
fi
if [ "$#" -ge 8 ]; then
    if [ "$8" = 'pico_plus' ]; then
        PICO_PLUS='pico_plus'
    elif [ "$8" = 'not_pico_plus' ]; then
        PICO_PLUS='not_pico_plus'
    else
        usage
        exit 1
    fi
else
    PICO_PLUS='not_pico_plus'
fi
if [ "$#" -eq 9 ]; then
    if [ "$9" = 'core_0' ]; then
        CORE=0
    elif [ "$9" = 'core_1' ]; then
        CORE=1
    else
        usage
        exit 1
    fi
else
    CORE=0
fi
BAUD=115200

utils/codeload3.sh -B ${BAUD} -p ${PORT} serial src/${PLATFORM}/forth/setup_full.fs
utils/load_cyw43_fw.sh ${PORT} ${FW} ${FW_CLM}
if [ ${ZEPTOIP} = 'ipv4' ]; then
    utils/codeload3.sh -B ${BAUD} -p ${PORT} serial extra/rp_common/pico_w_net_ipv4_all.fs
else
    utils/codeload3.sh -B ${BAUD} -p ${PORT} serial extra/rp_common/pico_w_net_ipv6_all.fs
fi
mkdir -p /tmp/picocalc
echo 'compile-to-flash' > /tmp/picocalc/prefix.fs
echo "${CORE} constant select-picocalc-tasks-core" >> /tmp/picocalc/prefix.fs
utils/codeload3.sh -B ${BAUD} -p ${PORT} serial /tmp/picocalc/prefix.fs
if [ ${TEXT_ONLY} = 'text' ]; then
    utils/codeload3.sh -B ${BAUD} -p ${PORT} serial extra/common/st7365p_spi_text_${FONT}_font_all.fs
else
    utils/codeload3.sh -B ${BAUD} -p ${PORT} serial extra/common/st7365p_spi_8_${FONT}_font_all.fs
fi
utils/codeload3.sh -B ${BAUD} -p ${PORT} serial extra/rp_common/picocalc_bios.fs
utils/codeload3.sh -B ${BAUD} -p ${PORT} serial extra/rp_common/picocalc_sound.fs
utils/codeload3.sh -B ${BAUD} -p ${PORT} serial extra/rp_common/picocalc_term_common.fs
if [ ${TEXT_ONLY} = 'text' ]; then
    utils/codeload3.sh -B ${BAUD} -p ${PORT} serial extra/rp_common/picocalc_term_text.fs
else
    utils/codeload3.sh -B ${BAUD} -p ${PORT} serial extra/rp_common/picocalc_term.fs
fi
if [ ${PLATFORM} != 'rp2040' ]; then
    utils/codeload3.sh -B ${BAUD} -p ${PORT} serial extra/common/zeptoed_all.fs
    utils/codeload3.sh -B ${BAUD} -p ${PORT} serial extra/common/transfer_all.fs
    utils/codeload3.sh -B ${BAUD} -p ${PORT} serial extra/common/inter_fs_copy.fs
fi
echo 'reboot' > /tmp/picocalc/suffix.fs
utils/codeload3.sh -B ${BAUD} -p ${PORT} serial /tmp/picocalc/suffix.fs
utils/codeload3.sh -B ${BAUD} -p ${PORT} serial extra/common/setup_blocks_fat32.fs
if [ ${PICO_PLUS} = 'pico_plus' ]; then
    utils/codeload3.sh -B ${BAUD} -p ${PORT} serial extra/rp2350/setup_pico_plus_2_psram_fat32.fs
fi
utils/codeload3.sh -B ${BAUD} -p ${PORT} serial extra/rp_common/picocalc_fat32.fs
echo 'compile-to-flash' > /tmp/picocalc/prefix.fs
utils/codeload3.sh -B ${BAUD} -p ${PORT} serial /tmp/picocalc/prefix.fs
if [ ${TEXT_ONLY} = 'text' ]; then
    utils/codeload3.sh -B ${BAUD} -p ${PORT} serial extra/rp_common/picocalc_screenshot_text.fs
else
    utils/codeload3.sh -B ${BAUD} -p ${PORT} serial extra/rp_common/picocalc_screenshot.fs
fi
echo 'initializer picocalc-term::term-console' > /tmp/picocalc/suffix.fs
echo 'cornerstone restore-picocalc' >> /tmp/picocalc/suffix.fs
echo 'reboot' >> /tmp/picocalc/suffix.fs
utils/codeload3.sh -B ${BAUD} -p ${PORT} serial /tmp/picocalc/suffix.fs
rm -rf /tmp/picocalc
