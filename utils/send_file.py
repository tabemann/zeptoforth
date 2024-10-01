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

PACKET_TIMEOUT = 5.0
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

class SendFile:
    def __init__(self, device, baud):
        self.device = device
        self.baud = baud
        try:
            self.port = serial.Serial(
                port = device,
                baudrate = baud,
                parity = serial.PARITY_NONE,
                stopbits = serial.STOPBITS_ONE,
                bytesize = serial.EIGHTBITS,
                timeout = PACKET_TIMEOUT)
            self.port.dtr = True
        except:
            print('Error: TTY device %s invalid' % device)
            sys.exit(1)

    def send_msg(self, data):
        self.port.write(data)
        self.port.flush()

    def send_done_packet(self):
        sent = False
        while not sent:
            body = b'';
            len_field = len(body).to_bytes(4, 'little')
            body_padded = body.ljust(PACKET_BODY_SIZE, b'\x00')
            head = SEND_PACKET_DONE
            packet_minus_crc = head + len_field + body_padded
            packet = packet_minus_crc + \
                     binascii.crc32(packet_minus_crc).to_bytes(4, 'little')
            print('Done')
            self.send_msg(base64.b64encode(packet))
            try:
                recv_packet = self.port.read(len(RECV_PACKET_ACK_FULL))
            except:
                print('Timed out')
                sys.exit(1)
            if recv_packet == RECV_PACKET_ACK_FULL:
                sent = True
    
    def send_data_packet(self, body):
        sent = False
        resending = False
        while not sent:
            len_field = len(body).to_bytes(4, 'little')
            body_padded = body.ljust(PACKET_BODY_SIZE, b'\x00')
            head = SEND_PACKET_NEXT
            if resending:
                head = SEND_PACKET_RESEND
            packet_minus_crc = head + len_field + body_padded
            packet = packet_minus_crc + \
                     binascii.crc32(packet_minus_crc).to_bytes(4, 'little')
            self.send_msg(base64.b64encode(packet))
            try:
                recv_packet = self.port.read(len(RECV_PACKET_ACK_FULL))
            except:
                print('Timed out')
                sys.exit(1)
            if recv_packet == RECV_PACKET_ACK_FULL:
                print('Sent %d bytes' % len(body))
                sent = True
            elif recv_packet == RECV_PACKET_NAK_FULL:
                print('Resending %d bytes' % len(body))
                resending = True
            elif recv_packet == RECV_PACKET_FAIL_FULL:
                print('Error sending data')
                self.send_done_packet()
                sys.exit(1)
            else:
                print('Unknown packet')

    def send_data(self, data):
        for index in range(0, len(data), PACKET_BODY_SIZE):
            self.send_data_packet(
                data[index:min(len(data), index + PACKET_BODY_SIZE)])
        self.send_done_packet()
        self.port.close()

def main():
    if len(sys.argv) != 4:
        print('Usage: %s <device> <baud> <file>' % sys.argv[0])
        sys.exit(1)
    data_file = open(sys.argv[3], 'rb')
    SendFile(sys.argv[1], int(sys.argv[2])).send_data(data_file.read())
    data_file.close()

if __name__ == '__main__':
    main()
