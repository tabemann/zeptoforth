#!/usr/bin/env python3

# Copyright (c) 2024 Travis Bemann
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

import sys
import serial
import binascii
import base64

PACKET_BODY_SIZE = 512
SEND_PACKET_DONE = (0).to_bytes(4, 'little')
SEND_PACKET_NEXT = (1).to_bytes(4, 'little')
SEND_PACKET_RESEND = (2).to_bytes(4, 'little')
RECV_PACKET_ACK = (3).to_bytes(4, 'little')
RECV_PACKET_NAK = (4).to_bytes(4, 'little')
RECV_PACKET_FAIL = (5).to_bytes(4, 'little')

def make_recv_packet(message):
    len_field = (0).to_bytes(4, 'little')
    packet_minus_crc = message + len_field
    packet = packet_minus_crc + \
             binascii.crc32(packet_minus_crc).to_bytes(4, 'little')
    return base64.b64encode(packet)

RECV_PACKET_ACK_FULL = make_recv_packet(RECV_PACKET_ACK)
RECV_PACKET_NAK_FULL = make_recv_packet(RECV_PACKET_NAK)
RECV_PACKET_FAIL_FULL = make_recv_packet(RECV_PACKET_FAIL)

def generate_send_packet_len():
    body = b'';
    len_field = len(body).to_bytes(4, 'little')
    body_padded = body.ljust(PACKET_BODY_SIZE, b'\x00')
    head = SEND_PACKET_DONE
    packet_minus_crc = head + len_field + body_padded
    packet = packet_minus_crc + \
             binascii.crc32(packet_minus_crc).to_bytes(4, 'little')
    return len(base64.b64encode(packet))

SEND_PACKET_LEN = generate_send_packet_len()

class RecvFile:
    def __init__(self, device, baud):
        self.device = device
        self.baud = baud
        try:
            self.port = serial.Serial(
                port = device,
                baudrate = baud,
                parity = serial.PARITY_NONE,
                stopbits = serial.STOPBITS_ONE,
                bytesize = serial.EIGHTBITS)
            self.port.dtr = True
        except:
            print('Error: TTY device %s invalid' % device)
            sys.exit(1)

    def send_msg(self, data):
        self.port.write(data)
        self.port.flush()

    def recv_data(self, data_file):
        self.send_msg(RECV_PACKET_ACK_FULL)
        done = False
        last_data = None
        while not done:
            packet = base64.b64decode(self.port.read(SEND_PACKET_LEN))
            head = packet[0:4]
            len_field = int.from_bytes(packet[4:8], 'little')
            body = packet[8:8 + min(len_field, PACKET_BODY_SIZE)]
            packet_minus_crc = packet[0:8 + PACKET_BODY_SIZE]
            crc = int.from_bytes(packet[8 + PACKET_BODY_SIZE:
                                        8 + PACKET_BODY_SIZE + 4],
                                 'little')
            if binascii.crc32(packet_minus_crc) == crc:
                if head == SEND_PACKET_DONE:
                    self.send_msg(RECV_PACKET_ACK_FULL)
                    print('Done')
                    done = True
                elif head == SEND_PACKET_NEXT:
                    if last_data != None:
                        data_file.write(last_data)
                    last_data = body
                    self.send_msg(RECV_PACKET_ACK_FULL)
                    print('Received %d bytes' % len_field)
                elif head == SEND_PACKET_RESEND:
                    last_data = body
                    self.send_msg(RECV_PACKET_ACK_FULL)
                    print('Re-received %d bytes' % len_field)
                else:
                    self.send_msg(RECV_PACKET_NAK_FULL)
                    print('Unknown packet')
            else:
                print('CRC doesn\'t match')
                self.send_msg(RECV_PACKET_NAK_FULL)
        if last_data != None:
            data_file.write(last_data)
        self.port.close()

def main():
    if len(sys.argv) != 4:
        print('Usage: %s <device> <baud> <file>' % sys.argv[0])
        sys.exit(1)
    data_file = open(sys.argv[3], 'wb')
    RecvFile(sys.argv[1], int(sys.argv[2])).recv_data(data_file)
    data_file.close()

if __name__ == '__main__':
    main()
