\ Copyright (c) 2016-2020 Terry Porter <terry@tjporter.com.au>
\ Copyright (c) 2020-2022 Travis Bemann
\ 
\ Permission is hereby granted, free of charge, to any person obtaining a copy
\ of this software and associated documentation files (the "Software"), to deal
\ in the Software without restriction, including without limitation the rights
\ to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
\ copies of the Software, and to permit persons to whom the Software is
\ furnished to do so, subject to the following conditions:
\ 
\ The above copyright notice and this permission notice shall be included in
\ all copies or substantial portions of the Software.
\ 
\ THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
\ IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
\ FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
\ AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
\ LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
\ OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
\ SOFTWARE.

compile-to-flash

begin-module memmap

  execute-defined? use-RNG [if]
    $50060800 constant RNG ( Random number generator ) 
    RNG $0 + constant RNG_CR ( read-write )  \ control register
    RNG $4 + constant RNG_SR (  )  \ status register
    RNG $8 + constant RNG_DR ( read-only )  \ data register
    : RNG_CR. cr ." RNG_CR.  RW   $" RNG_CR @ hex. RNG_CR 1b. ;
    : RNG_SR. cr ." RNG_SR.   $" RNG_SR @ hex. RNG_SR 1b. ;
    : RNG_DR. cr ." RNG_DR.  RO   $" RNG_DR @ hex. RNG_DR 1b. ;
    : RNG.
      RNG_CR.
      RNG_SR.
      RNG_DR.
    ;
  [then]

  execute-defined? use-HASH [if]
    $50060400 constant HASH ( Hash processor ) 
    HASH $0 + constant HASH_CR (  )  \ control register
    HASH $4 + constant HASH_DIN ( read-write )  \ data input register
    HASH $8 + constant HASH_STR (  )  \ start register
    HASH $C + constant HASH_HR0 ( read-only )  \ digest registers
    HASH $10 + constant HASH_HR1 ( read-only )  \ digest registers
    HASH $14 + constant HASH_HR2 ( read-only )  \ digest registers
    HASH $18 + constant HASH_HR3 ( read-only )  \ digest registers
    HASH $1C + constant HASH_HR4 ( read-only )  \ digest registers
    HASH $20 + constant HASH_IMR ( read-write )  \ interrupt enable register
    HASH $24 + constant HASH_SR (  )  \ status register
    HASH $F8 + constant HASH_CSR0 ( read-write )  \ context swap registers
    HASH $FC + constant HASH_CSR1 ( read-write )  \ context swap registers
    HASH $100 + constant HASH_CSR2 ( read-write )  \ context swap registers
    HASH $104 + constant HASH_CSR3 ( read-write )  \ context swap registers
    HASH $108 + constant HASH_CSR4 ( read-write )  \ context swap registers
    HASH $10C + constant HASH_CSR5 ( read-write )  \ context swap registers
    HASH $110 + constant HASH_CSR6 ( read-write )  \ context swap registers
    HASH $114 + constant HASH_CSR7 ( read-write )  \ context swap registers
    HASH $118 + constant HASH_CSR8 ( read-write )  \ context swap registers
    HASH $11C + constant HASH_CSR9 ( read-write )  \ context swap registers
    HASH $120 + constant HASH_CSR10 ( read-write )  \ context swap registers
    HASH $124 + constant HASH_CSR11 ( read-write )  \ context swap registers
    HASH $128 + constant HASH_CSR12 ( read-write )  \ context swap registers
    HASH $12C + constant HASH_CSR13 ( read-write )  \ context swap registers
    HASH $130 + constant HASH_CSR14 ( read-write )  \ context swap registers
    HASH $134 + constant HASH_CSR15 ( read-write )  \ context swap registers
    HASH $138 + constant HASH_CSR16 ( read-write )  \ context swap registers
    HASH $13C + constant HASH_CSR17 ( read-write )  \ context swap registers
    HASH $140 + constant HASH_CSR18 ( read-write )  \ context swap registers
    HASH $144 + constant HASH_CSR19 ( read-write )  \ context swap registers
    HASH $148 + constant HASH_CSR20 ( read-write )  \ context swap registers
    HASH $14C + constant HASH_CSR21 ( read-write )  \ context swap registers
    HASH $150 + constant HASH_CSR22 ( read-write )  \ context swap registers
    HASH $154 + constant HASH_CSR23 ( read-write )  \ context swap registers
    HASH $158 + constant HASH_CSR24 ( read-write )  \ context swap registers
    HASH $15C + constant HASH_CSR25 ( read-write )  \ context swap registers
    HASH $160 + constant HASH_CSR26 ( read-write )  \ context swap registers
    HASH $164 + constant HASH_CSR27 ( read-write )  \ context swap registers
    HASH $168 + constant HASH_CSR28 ( read-write )  \ context swap registers
    HASH $16C + constant HASH_CSR29 ( read-write )  \ context swap registers
    HASH $170 + constant HASH_CSR30 ( read-write )  \ context swap registers
    HASH $174 + constant HASH_CSR31 ( read-write )  \ context swap registers
    HASH $178 + constant HASH_CSR32 ( read-write )  \ context swap registers
    HASH $17C + constant HASH_CSR33 ( read-write )  \ context swap registers
    HASH $180 + constant HASH_CSR34 ( read-write )  \ context swap registers
    HASH $184 + constant HASH_CSR35 ( read-write )  \ context swap registers
    HASH $188 + constant HASH_CSR36 ( read-write )  \ context swap registers
    HASH $18C + constant HASH_CSR37 ( read-write )  \ context swap registers
    HASH $190 + constant HASH_CSR38 ( read-write )  \ context swap registers
    HASH $194 + constant HASH_CSR39 ( read-write )  \ context swap registers
    HASH $198 + constant HASH_CSR40 ( read-write )  \ context swap registers
    HASH $19C + constant HASH_CSR41 ( read-write )  \ context swap registers
    HASH $1A0 + constant HASH_CSR42 ( read-write )  \ context swap registers
    HASH $1A4 + constant HASH_CSR43 ( read-write )  \ context swap registers
    HASH $1A8 + constant HASH_CSR44 ( read-write )  \ context swap registers
    HASH $1AC + constant HASH_CSR45 ( read-write )  \ context swap registers
    HASH $1B0 + constant HASH_CSR46 ( read-write )  \ context swap registers
    HASH $1B4 + constant HASH_CSR47 ( read-write )  \ context swap registers
    HASH $1B8 + constant HASH_CSR48 ( read-write )  \ context swap registers
    HASH $1BC + constant HASH_CSR49 ( read-write )  \ context swap registers
    HASH $1C0 + constant HASH_CSR50 ( read-write )  \ context swap registers
    HASH $1C4 + constant HASH_CSR51 ( read-write )  \ context swap registers
    HASH $1C8 + constant HASH_CSR52 ( read-write )  \ context swap registers
    HASH $1CC + constant HASH_CSR53 ( read-write )  \ context swap registers
    HASH $310 + constant HASH_HASH_HR0 ( read-only )  \ HASH digest register
    HASH $314 + constant HASH_HASH_HR1 ( read-only )  \ read-only
    HASH $318 + constant HASH_HASH_HR2 ( read-only )  \ read-only
    HASH $31C + constant HASH_HASH_HR3 ( read-only )  \ read-only
    HASH $320 + constant HASH_HASH_HR4 ( read-only )  \ read-only
    HASH $324 + constant HASH_HASH_HR5 ( read-only )  \ read-only
    HASH $328 + constant HASH_HASH_HR6 ( read-only )  \ read-only
    HASH $32C + constant HASH_HASH_HR7 ( read-only )  \ read-only
    : HASH_CR. cr ." HASH_CR.   $" HASH_CR @ hex. HASH_CR 1b. ;
    : HASH_DIN. cr ." HASH_DIN.  RW   $" HASH_DIN @ hex. HASH_DIN 1b. ;
    : HASH_STR. cr ." HASH_STR.   $" HASH_STR @ hex. HASH_STR 1b. ;
    : HASH_HR0. cr ." HASH_HR0.  RO   $" HASH_HR0 @ hex. HASH_HR0 1b. ;
    : HASH_HR1. cr ." HASH_HR1.  RO   $" HASH_HR1 @ hex. HASH_HR1 1b. ;
    : HASH_HR2. cr ." HASH_HR2.  RO   $" HASH_HR2 @ hex. HASH_HR2 1b. ;
    : HASH_HR3. cr ." HASH_HR3.  RO   $" HASH_HR3 @ hex. HASH_HR3 1b. ;
    : HASH_HR4. cr ." HASH_HR4.  RO   $" HASH_HR4 @ hex. HASH_HR4 1b. ;
    : HASH_IMR. cr ." HASH_IMR.  RW   $" HASH_IMR @ hex. HASH_IMR 1b. ;
    : HASH_SR. cr ." HASH_SR.   $" HASH_SR @ hex. HASH_SR 1b. ;
    : HASH_CSR0. cr ." HASH_CSR0.  RW   $" HASH_CSR0 @ hex. HASH_CSR0 1b. ;
    : HASH_CSR1. cr ." HASH_CSR1.  RW   $" HASH_CSR1 @ hex. HASH_CSR1 1b. ;
    : HASH_CSR2. cr ." HASH_CSR2.  RW   $" HASH_CSR2 @ hex. HASH_CSR2 1b. ;
    : HASH_CSR3. cr ." HASH_CSR3.  RW   $" HASH_CSR3 @ hex. HASH_CSR3 1b. ;
    : HASH_CSR4. cr ." HASH_CSR4.  RW   $" HASH_CSR4 @ hex. HASH_CSR4 1b. ;
    : HASH_CSR5. cr ." HASH_CSR5.  RW   $" HASH_CSR5 @ hex. HASH_CSR5 1b. ;
    : HASH_CSR6. cr ." HASH_CSR6.  RW   $" HASH_CSR6 @ hex. HASH_CSR6 1b. ;
    : HASH_CSR7. cr ." HASH_CSR7.  RW   $" HASH_CSR7 @ hex. HASH_CSR7 1b. ;
    : HASH_CSR8. cr ." HASH_CSR8.  RW   $" HASH_CSR8 @ hex. HASH_CSR8 1b. ;
    : HASH_CSR9. cr ." HASH_CSR9.  RW   $" HASH_CSR9 @ hex. HASH_CSR9 1b. ;
    : HASH_CSR10. cr ." HASH_CSR10.  RW   $" HASH_CSR10 @ hex. HASH_CSR10 1b. ;
    : HASH_CSR11. cr ." HASH_CSR11.  RW   $" HASH_CSR11 @ hex. HASH_CSR11 1b. ;
    : HASH_CSR12. cr ." HASH_CSR12.  RW   $" HASH_CSR12 @ hex. HASH_CSR12 1b. ;
    : HASH_CSR13. cr ." HASH_CSR13.  RW   $" HASH_CSR13 @ hex. HASH_CSR13 1b. ;
    : HASH_CSR14. cr ." HASH_CSR14.  RW   $" HASH_CSR14 @ hex. HASH_CSR14 1b. ;
    : HASH_CSR15. cr ." HASH_CSR15.  RW   $" HASH_CSR15 @ hex. HASH_CSR15 1b. ;
    : HASH_CSR16. cr ." HASH_CSR16.  RW   $" HASH_CSR16 @ hex. HASH_CSR16 1b. ;
    : HASH_CSR17. cr ." HASH_CSR17.  RW   $" HASH_CSR17 @ hex. HASH_CSR17 1b. ;
    : HASH_CSR18. cr ." HASH_CSR18.  RW   $" HASH_CSR18 @ hex. HASH_CSR18 1b. ;
    : HASH_CSR19. cr ." HASH_CSR19.  RW   $" HASH_CSR19 @ hex. HASH_CSR19 1b. ;
    : HASH_CSR20. cr ." HASH_CSR20.  RW   $" HASH_CSR20 @ hex. HASH_CSR20 1b. ;
    : HASH_CSR21. cr ." HASH_CSR21.  RW   $" HASH_CSR21 @ hex. HASH_CSR21 1b. ;
    : HASH_CSR22. cr ." HASH_CSR22.  RW   $" HASH_CSR22 @ hex. HASH_CSR22 1b. ;
    : HASH_CSR23. cr ." HASH_CSR23.  RW   $" HASH_CSR23 @ hex. HASH_CSR23 1b. ;
    : HASH_CSR24. cr ." HASH_CSR24.  RW   $" HASH_CSR24 @ hex. HASH_CSR24 1b. ;
    : HASH_CSR25. cr ." HASH_CSR25.  RW   $" HASH_CSR25 @ hex. HASH_CSR25 1b. ;
    : HASH_CSR26. cr ." HASH_CSR26.  RW   $" HASH_CSR26 @ hex. HASH_CSR26 1b. ;
    : HASH_CSR27. cr ." HASH_CSR27.  RW   $" HASH_CSR27 @ hex. HASH_CSR27 1b. ;
    : HASH_CSR28. cr ." HASH_CSR28.  RW   $" HASH_CSR28 @ hex. HASH_CSR28 1b. ;
    : HASH_CSR29. cr ." HASH_CSR29.  RW   $" HASH_CSR29 @ hex. HASH_CSR29 1b. ;
    : HASH_CSR30. cr ." HASH_CSR30.  RW   $" HASH_CSR30 @ hex. HASH_CSR30 1b. ;
    : HASH_CSR31. cr ." HASH_CSR31.  RW   $" HASH_CSR31 @ hex. HASH_CSR31 1b. ;
    : HASH_CSR32. cr ." HASH_CSR32.  RW   $" HASH_CSR32 @ hex. HASH_CSR32 1b. ;
    : HASH_CSR33. cr ." HASH_CSR33.  RW   $" HASH_CSR33 @ hex. HASH_CSR33 1b. ;
    : HASH_CSR34. cr ." HASH_CSR34.  RW   $" HASH_CSR34 @ hex. HASH_CSR34 1b. ;
    : HASH_CSR35. cr ." HASH_CSR35.  RW   $" HASH_CSR35 @ hex. HASH_CSR35 1b. ;
    : HASH_CSR36. cr ." HASH_CSR36.  RW   $" HASH_CSR36 @ hex. HASH_CSR36 1b. ;
    : HASH_CSR37. cr ." HASH_CSR37.  RW   $" HASH_CSR37 @ hex. HASH_CSR37 1b. ;
    : HASH_CSR38. cr ." HASH_CSR38.  RW   $" HASH_CSR38 @ hex. HASH_CSR38 1b. ;
    : HASH_CSR39. cr ." HASH_CSR39.  RW   $" HASH_CSR39 @ hex. HASH_CSR39 1b. ;
    : HASH_CSR40. cr ." HASH_CSR40.  RW   $" HASH_CSR40 @ hex. HASH_CSR40 1b. ;
    : HASH_CSR41. cr ." HASH_CSR41.  RW   $" HASH_CSR41 @ hex. HASH_CSR41 1b. ;
    : HASH_CSR42. cr ." HASH_CSR42.  RW   $" HASH_CSR42 @ hex. HASH_CSR42 1b. ;
    : HASH_CSR43. cr ." HASH_CSR43.  RW   $" HASH_CSR43 @ hex. HASH_CSR43 1b. ;
    : HASH_CSR44. cr ." HASH_CSR44.  RW   $" HASH_CSR44 @ hex. HASH_CSR44 1b. ;
    : HASH_CSR45. cr ." HASH_CSR45.  RW   $" HASH_CSR45 @ hex. HASH_CSR45 1b. ;
    : HASH_CSR46. cr ." HASH_CSR46.  RW   $" HASH_CSR46 @ hex. HASH_CSR46 1b. ;
    : HASH_CSR47. cr ." HASH_CSR47.  RW   $" HASH_CSR47 @ hex. HASH_CSR47 1b. ;
    : HASH_CSR48. cr ." HASH_CSR48.  RW   $" HASH_CSR48 @ hex. HASH_CSR48 1b. ;
    : HASH_CSR49. cr ." HASH_CSR49.  RW   $" HASH_CSR49 @ hex. HASH_CSR49 1b. ;
    : HASH_CSR50. cr ." HASH_CSR50.  RW   $" HASH_CSR50 @ hex. HASH_CSR50 1b. ;
    : HASH_CSR51. cr ." HASH_CSR51.  RW   $" HASH_CSR51 @ hex. HASH_CSR51 1b. ;
    : HASH_CSR52. cr ." HASH_CSR52.  RW   $" HASH_CSR52 @ hex. HASH_CSR52 1b. ;
    : HASH_CSR53. cr ." HASH_CSR53.  RW   $" HASH_CSR53 @ hex. HASH_CSR53 1b. ;
    : HASH_HASH_HR0. cr ." HASH_HASH_HR0.  RO   $" HASH_HASH_HR0 @ hex. HASH_HASH_HR0 1b. ;
    : HASH_HASH_HR1. cr ." HASH_HASH_HR1.  RO   $" HASH_HASH_HR1 @ hex. HASH_HASH_HR1 1b. ;
    : HASH_HASH_HR2. cr ." HASH_HASH_HR2.  RO   $" HASH_HASH_HR2 @ hex. HASH_HASH_HR2 1b. ;
    : HASH_HASH_HR3. cr ." HASH_HASH_HR3.  RO   $" HASH_HASH_HR3 @ hex. HASH_HASH_HR3 1b. ;
    : HASH_HASH_HR4. cr ." HASH_HASH_HR4.  RO   $" HASH_HASH_HR4 @ hex. HASH_HASH_HR4 1b. ;
    : HASH_HASH_HR5. cr ." HASH_HASH_HR5.  RO   $" HASH_HASH_HR5 @ hex. HASH_HASH_HR5 1b. ;
    : HASH_HASH_HR6. cr ." HASH_HASH_HR6.  RO   $" HASH_HASH_HR6 @ hex. HASH_HASH_HR6 1b. ;
    : HASH_HASH_HR7. cr ." HASH_HASH_HR7.  RO   $" HASH_HASH_HR7 @ hex. HASH_HASH_HR7 1b. ;
    : HASH.
      HASH_CR.
      HASH_DIN.
      HASH_STR.
      HASH_HR0.
      HASH_HR1.
      HASH_HR2.
      HASH_HR3.
      HASH_HR4.
      HASH_IMR.
      HASH_SR.
      HASH_CSR0.
      HASH_CSR1.
      HASH_CSR2.
      HASH_CSR3.
      HASH_CSR4.
      HASH_CSR5.
      HASH_CSR6.
      HASH_CSR7.
      HASH_CSR8.
      HASH_CSR9.
      HASH_CSR10.
      HASH_CSR11.
      HASH_CSR12.
      HASH_CSR13.
      HASH_CSR14.
      HASH_CSR15.
      HASH_CSR16.
      HASH_CSR17.
      HASH_CSR18.
      HASH_CSR19.
      HASH_CSR20.
      HASH_CSR21.
      HASH_CSR22.
      HASH_CSR23.
      HASH_CSR24.
      HASH_CSR25.
      HASH_CSR26.
      HASH_CSR27.
      HASH_CSR28.
      HASH_CSR29.
      HASH_CSR30.
      HASH_CSR31.
      HASH_CSR32.
      HASH_CSR33.
      HASH_CSR34.
      HASH_CSR35.
      HASH_CSR36.
      HASH_CSR37.
      HASH_CSR38.
      HASH_CSR39.
      HASH_CSR40.
      HASH_CSR41.
      HASH_CSR42.
      HASH_CSR43.
      HASH_CSR44.
      HASH_CSR45.
      HASH_CSR46.
      HASH_CSR47.
      HASH_CSR48.
      HASH_CSR49.
      HASH_CSR50.
      HASH_CSR51.
      HASH_CSR52.
      HASH_CSR53.
      HASH_HASH_HR0.
      HASH_HASH_HR1.
      HASH_HASH_HR2.
      HASH_HASH_HR3.
      HASH_HASH_HR4.
      HASH_HASH_HR5.
      HASH_HASH_HR6.
      HASH_HASH_HR7.
    ;
  [then]

  execute-defined? use-CRYP [if]
    $50060000 constant CRYP ( Cryptographic processor ) 
    CRYP $0 + constant CRYP_CR (  )  \ control register
    CRYP $4 + constant CRYP_SR ( read-only )  \ status register
    CRYP $8 + constant CRYP_DIN ( read-write )  \ data input register
    CRYP $C + constant CRYP_DOUT ( read-only )  \ data output register
    CRYP $10 + constant CRYP_DMACR ( read-write )  \ DMA control register
    CRYP $14 + constant CRYP_IMSCR ( read-write )  \ interrupt mask set/clear register
    CRYP $18 + constant CRYP_RISR ( read-only )  \ raw interrupt status register
    CRYP $1C + constant CRYP_MISR ( read-only )  \ masked interrupt status register
    CRYP $20 + constant CRYP_K0LR ( write-only )  \ key registers
    CRYP $24 + constant CRYP_K0RR ( write-only )  \ key registers
    CRYP $28 + constant CRYP_K1LR ( write-only )  \ key registers
    CRYP $2C + constant CRYP_K1RR ( write-only )  \ key registers
    CRYP $30 + constant CRYP_K2LR ( write-only )  \ key registers
    CRYP $34 + constant CRYP_K2RR ( write-only )  \ key registers
    CRYP $38 + constant CRYP_K3LR ( write-only )  \ key registers
    CRYP $3C + constant CRYP_K3RR ( write-only )  \ key registers
    CRYP $40 + constant CRYP_IV0LR ( read-write )  \ initialization vector registers
    CRYP $44 + constant CRYP_IV0RR ( read-write )  \ initialization vector registers
    CRYP $48 + constant CRYP_IV1LR ( read-write )  \ initialization vector registers
    CRYP $4C + constant CRYP_IV1RR ( read-write )  \ initialization vector registers
    CRYP $50 + constant CRYP_CSGCMCCM0R ( read-write )  \ context swap register
    CRYP $54 + constant CRYP_CSGCMCCM1R ( read-write )  \ context swap register
    CRYP $58 + constant CRYP_CSGCMCCM2R ( read-write )  \ context swap register
    CRYP $5C + constant CRYP_CSGCMCCM3R ( read-write )  \ context swap register
    CRYP $60 + constant CRYP_CSGCMCCM4R ( read-write )  \ context swap register
    CRYP $64 + constant CRYP_CSGCMCCM5R ( read-write )  \ context swap register
    CRYP $68 + constant CRYP_CSGCMCCM6R ( read-write )  \ context swap register
    CRYP $6C + constant CRYP_CSGCMCCM7R ( read-write )  \ context swap register
    CRYP $70 + constant CRYP_CSGCM0R ( read-write )  \ context swap register
    CRYP $74 + constant CRYP_CSGCM1R ( read-write )  \ context swap register
    CRYP $78 + constant CRYP_CSGCM2R ( read-write )  \ context swap register
    CRYP $7C + constant CRYP_CSGCM3R ( read-write )  \ context swap register
    CRYP $80 + constant CRYP_CSGCM4R ( read-write )  \ context swap register
    CRYP $84 + constant CRYP_CSGCM5R ( read-write )  \ context swap register
    CRYP $88 + constant CRYP_CSGCM6R ( read-write )  \ context swap register
    CRYP $8C + constant CRYP_CSGCM7R ( read-write )  \ context swap register
    : CRYP_CR. cr ." CRYP_CR.   $" CRYP_CR @ hex. CRYP_CR 1b. ;
    : CRYP_SR. cr ." CRYP_SR.  RO   $" CRYP_SR @ hex. CRYP_SR 1b. ;
    : CRYP_DIN. cr ." CRYP_DIN.  RW   $" CRYP_DIN @ hex. CRYP_DIN 1b. ;
    : CRYP_DOUT. cr ." CRYP_DOUT.  RO   $" CRYP_DOUT @ hex. CRYP_DOUT 1b. ;
    : CRYP_DMACR. cr ." CRYP_DMACR.  RW   $" CRYP_DMACR @ hex. CRYP_DMACR 1b. ;
    : CRYP_IMSCR. cr ." CRYP_IMSCR.  RW   $" CRYP_IMSCR @ hex. CRYP_IMSCR 1b. ;
    : CRYP_RISR. cr ." CRYP_RISR.  RO   $" CRYP_RISR @ hex. CRYP_RISR 1b. ;
    : CRYP_MISR. cr ." CRYP_MISR.  RO   $" CRYP_MISR @ hex. CRYP_MISR 1b. ;
    : CRYP_K0LR. cr ." CRYP_K0LR " WRITEONLY ; 
    : CRYP_K0RR. cr ." CRYP_K0RR " WRITEONLY ; 
    : CRYP_K1LR. cr ." CRYP_K1LR " WRITEONLY ; 
    : CRYP_K1RR. cr ." CRYP_K1RR " WRITEONLY ; 
    : CRYP_K2LR. cr ." CRYP_K2LR " WRITEONLY ; 
    : CRYP_K2RR. cr ." CRYP_K2RR " WRITEONLY ; 
    : CRYP_K3LR. cr ." CRYP_K3LR " WRITEONLY ; 
    : CRYP_K3RR. cr ." CRYP_K3RR " WRITEONLY ; 
    : CRYP_IV0LR. cr ." CRYP_IV0LR.  RW   $" CRYP_IV0LR @ hex. CRYP_IV0LR 1b. ;
    : CRYP_IV0RR. cr ." CRYP_IV0RR.  RW   $" CRYP_IV0RR @ hex. CRYP_IV0RR 1b. ;
    : CRYP_IV1LR. cr ." CRYP_IV1LR.  RW   $" CRYP_IV1LR @ hex. CRYP_IV1LR 1b. ;
    : CRYP_IV1RR. cr ." CRYP_IV1RR.  RW   $" CRYP_IV1RR @ hex. CRYP_IV1RR 1b. ;
    : CRYP_CSGCMCCM0R. cr ." CRYP_CSGCMCCM0R.  RW   $" CRYP_CSGCMCCM0R @ hex. CRYP_CSGCMCCM0R 1b. ;
    : CRYP_CSGCMCCM1R. cr ." CRYP_CSGCMCCM1R.  RW   $" CRYP_CSGCMCCM1R @ hex. CRYP_CSGCMCCM1R 1b. ;
    : CRYP_CSGCMCCM2R. cr ." CRYP_CSGCMCCM2R.  RW   $" CRYP_CSGCMCCM2R @ hex. CRYP_CSGCMCCM2R 1b. ;
    : CRYP_CSGCMCCM3R. cr ." CRYP_CSGCMCCM3R.  RW   $" CRYP_CSGCMCCM3R @ hex. CRYP_CSGCMCCM3R 1b. ;
    : CRYP_CSGCMCCM4R. cr ." CRYP_CSGCMCCM4R.  RW   $" CRYP_CSGCMCCM4R @ hex. CRYP_CSGCMCCM4R 1b. ;
    : CRYP_CSGCMCCM5R. cr ." CRYP_CSGCMCCM5R.  RW   $" CRYP_CSGCMCCM5R @ hex. CRYP_CSGCMCCM5R 1b. ;
    : CRYP_CSGCMCCM6R. cr ." CRYP_CSGCMCCM6R.  RW   $" CRYP_CSGCMCCM6R @ hex. CRYP_CSGCMCCM6R 1b. ;
    : CRYP_CSGCMCCM7R. cr ." CRYP_CSGCMCCM7R.  RW   $" CRYP_CSGCMCCM7R @ hex. CRYP_CSGCMCCM7R 1b. ;
    : CRYP_CSGCM0R. cr ." CRYP_CSGCM0R.  RW   $" CRYP_CSGCM0R @ hex. CRYP_CSGCM0R 1b. ;
    : CRYP_CSGCM1R. cr ." CRYP_CSGCM1R.  RW   $" CRYP_CSGCM1R @ hex. CRYP_CSGCM1R 1b. ;
    : CRYP_CSGCM2R. cr ." CRYP_CSGCM2R.  RW   $" CRYP_CSGCM2R @ hex. CRYP_CSGCM2R 1b. ;
    : CRYP_CSGCM3R. cr ." CRYP_CSGCM3R.  RW   $" CRYP_CSGCM3R @ hex. CRYP_CSGCM3R 1b. ;
    : CRYP_CSGCM4R. cr ." CRYP_CSGCM4R.  RW   $" CRYP_CSGCM4R @ hex. CRYP_CSGCM4R 1b. ;
    : CRYP_CSGCM5R. cr ." CRYP_CSGCM5R.  RW   $" CRYP_CSGCM5R @ hex. CRYP_CSGCM5R 1b. ;
    : CRYP_CSGCM6R. cr ." CRYP_CSGCM6R.  RW   $" CRYP_CSGCM6R @ hex. CRYP_CSGCM6R 1b. ;
    : CRYP_CSGCM7R. cr ." CRYP_CSGCM7R.  RW   $" CRYP_CSGCM7R @ hex. CRYP_CSGCM7R 1b. ;
    : CRYP.
      CRYP_CR.
      CRYP_SR.
      CRYP_DIN.
      CRYP_DOUT.
      CRYP_DMACR.
      CRYP_IMSCR.
      CRYP_RISR.
      CRYP_MISR.
      CRYP_K0LR.
      CRYP_K0RR.
      CRYP_K1LR.
      CRYP_K1RR.
      CRYP_K2LR.
      CRYP_K2RR.
      CRYP_K3LR.
      CRYP_K3RR.
      CRYP_IV0LR.
      CRYP_IV0RR.
      CRYP_IV1LR.
      CRYP_IV1RR.
      CRYP_CSGCMCCM0R.
      CRYP_CSGCMCCM1R.
      CRYP_CSGCMCCM2R.
      CRYP_CSGCMCCM3R.
      CRYP_CSGCMCCM4R.
      CRYP_CSGCMCCM5R.
      CRYP_CSGCMCCM6R.
      CRYP_CSGCMCCM7R.
      CRYP_CSGCM0R.
      CRYP_CSGCM1R.
      CRYP_CSGCM2R.
      CRYP_CSGCM3R.
      CRYP_CSGCM4R.
      CRYP_CSGCM5R.
      CRYP_CSGCM6R.
      CRYP_CSGCM7R.
    ;
  [then]

  execute-defined? use-DCMI [if]
    $50050000 constant DCMI ( Digital camera interface ) 
    DCMI $0 + constant DCMI_CR ( read-write )  \ control register 1
    DCMI $4 + constant DCMI_SR ( read-only )  \ status register
    DCMI $8 + constant DCMI_RIS ( read-only )  \ raw interrupt status register
    DCMI $C + constant DCMI_IER ( read-write )  \ interrupt enable register
    DCMI $10 + constant DCMI_MIS ( read-only )  \ masked interrupt status register
    DCMI $14 + constant DCMI_ICR ( write-only )  \ interrupt clear register
    DCMI $18 + constant DCMI_ESCR ( read-write )  \ embedded synchronization code register
    DCMI $1C + constant DCMI_ESUR ( read-write )  \ embedded synchronization unmask register
    DCMI $20 + constant DCMI_CWSTRT ( read-write )  \ crop window start
    DCMI $24 + constant DCMI_CWSIZE ( read-write )  \ crop window size
    DCMI $28 + constant DCMI_DR ( read-only )  \ data register
    : DCMI_CR. cr ." DCMI_CR.  RW   $" DCMI_CR @ hex. DCMI_CR 1b. ;
    : DCMI_SR. cr ." DCMI_SR.  RO   $" DCMI_SR @ hex. DCMI_SR 1b. ;
    : DCMI_RIS. cr ." DCMI_RIS.  RO   $" DCMI_RIS @ hex. DCMI_RIS 1b. ;
    : DCMI_IER. cr ." DCMI_IER.  RW   $" DCMI_IER @ hex. DCMI_IER 1b. ;
    : DCMI_MIS. cr ." DCMI_MIS.  RO   $" DCMI_MIS @ hex. DCMI_MIS 1b. ;
    : DCMI_ICR. cr ." DCMI_ICR " WRITEONLY ; 
    : DCMI_ESCR. cr ." DCMI_ESCR.  RW   $" DCMI_ESCR @ hex. DCMI_ESCR 1b. ;
    : DCMI_ESUR. cr ." DCMI_ESUR.  RW   $" DCMI_ESUR @ hex. DCMI_ESUR 1b. ;
    : DCMI_CWSTRT. cr ." DCMI_CWSTRT.  RW   $" DCMI_CWSTRT @ hex. DCMI_CWSTRT 1b. ;
    : DCMI_CWSIZE. cr ." DCMI_CWSIZE.  RW   $" DCMI_CWSIZE @ hex. DCMI_CWSIZE 1b. ;
    : DCMI_DR. cr ." DCMI_DR.  RO   $" DCMI_DR @ hex. DCMI_DR 1b. ;
    : DCMI.
      DCMI_CR.
      DCMI_SR.
      DCMI_RIS.
      DCMI_IER.
      DCMI_MIS.
      DCMI_ICR.
      DCMI_ESCR.
      DCMI_ESUR.
      DCMI_CWSTRT.
      DCMI_CWSIZE.
      DCMI_DR.
    ;
  [then]

  execute-defined? use-FMC [if]
    $A0000000 constant FMC ( Flexible memory controller ) 
    FMC $0 + constant FMC_BCR1 ( read-write )  \ SRAM/NOR-Flash chip-select control register 1
    FMC $4 + constant FMC_BTR1 ( read-write )  \ SRAM/NOR-Flash chip-select timing register 1
    FMC $8 + constant FMC_BCR2 ( read-write )  \ SRAM/NOR-Flash chip-select control register 2
    FMC $C + constant FMC_BTR2 ( read-write )  \ SRAM/NOR-Flash chip-select timing register 2
    FMC $10 + constant FMC_BCR3 ( read-write )  \ SRAM/NOR-Flash chip-select control register 3
    FMC $14 + constant FMC_BTR3 ( read-write )  \ SRAM/NOR-Flash chip-select timing register 3
    FMC $18 + constant FMC_BCR4 ( read-write )  \ SRAM/NOR-Flash chip-select control register 4
    FMC $1C + constant FMC_BTR4 ( read-write )  \ SRAM/NOR-Flash chip-select timing register 4
    FMC $80 + constant FMC_PCR ( read-write )  \ PC Card/NAND Flash control register
    FMC $84 + constant FMC_SR (  )  \ FIFO status and interrupt register
    FMC $88 + constant FMC_PMEM ( read-write )  \ Common memory space timing register
    FMC $8C + constant FMC_PATT ( read-write )  \ Attribute memory space timing register
    FMC $94 + constant FMC_ECCR ( read-only )  \ ECC result register
    FMC $104 + constant FMC_BWTR1 ( read-write )  \ SRAM/NOR-Flash write timing registers 1
    FMC $10C + constant FMC_BWTR2 ( read-write )  \ SRAM/NOR-Flash write timing registers 2
    FMC $114 + constant FMC_BWTR3 ( read-write )  \ SRAM/NOR-Flash write timing registers 3
    FMC $11C + constant FMC_BWTR4 ( read-write )  \ SRAM/NOR-Flash write timing registers 4
    FMC $140 + constant FMC_SDCR1 ( read-write )  \ SDRAM Control Register 1
    FMC $144 + constant FMC_SDCR2 ( read-write )  \ SDRAM Control Register 2
    FMC $148 + constant FMC_SDTR1 ( read-write )  \ SDRAM Timing register 1
    FMC $14C + constant FMC_SDTR2 ( read-write )  \ SDRAM Timing register 2
    FMC $150 + constant FMC_SDCMR (  )  \ SDRAM Command Mode register
    FMC $154 + constant FMC_SDRTR (  )  \ SDRAM Refresh Timer register
    FMC $158 + constant FMC_SDSR ( read-only )  \ SDRAM Status register
    : FMC_BCR1. cr ." FMC_BCR1.  RW   $" FMC_BCR1 @ hex. FMC_BCR1 1b. ;
    : FMC_BTR1. cr ." FMC_BTR1.  RW   $" FMC_BTR1 @ hex. FMC_BTR1 1b. ;
    : FMC_BCR2. cr ." FMC_BCR2.  RW   $" FMC_BCR2 @ hex. FMC_BCR2 1b. ;
    : FMC_BTR2. cr ." FMC_BTR2.  RW   $" FMC_BTR2 @ hex. FMC_BTR2 1b. ;
    : FMC_BCR3. cr ." FMC_BCR3.  RW   $" FMC_BCR3 @ hex. FMC_BCR3 1b. ;
    : FMC_BTR3. cr ." FMC_BTR3.  RW   $" FMC_BTR3 @ hex. FMC_BTR3 1b. ;
    : FMC_BCR4. cr ." FMC_BCR4.  RW   $" FMC_BCR4 @ hex. FMC_BCR4 1b. ;
    : FMC_BTR4. cr ." FMC_BTR4.  RW   $" FMC_BTR4 @ hex. FMC_BTR4 1b. ;
    : FMC_PCR. cr ." FMC_PCR.  RW   $" FMC_PCR @ hex. FMC_PCR 1b. ;
    : FMC_SR. cr ." FMC_SR.   $" FMC_SR @ hex. FMC_SR 1b. ;
    : FMC_PMEM. cr ." FMC_PMEM.  RW   $" FMC_PMEM @ hex. FMC_PMEM 1b. ;
    : FMC_PATT. cr ." FMC_PATT.  RW   $" FMC_PATT @ hex. FMC_PATT 1b. ;
    : FMC_ECCR. cr ." FMC_ECCR.  RO   $" FMC_ECCR @ hex. FMC_ECCR 1b. ;
    : FMC_BWTR1. cr ." FMC_BWTR1.  RW   $" FMC_BWTR1 @ hex. FMC_BWTR1 1b. ;
    : FMC_BWTR2. cr ." FMC_BWTR2.  RW   $" FMC_BWTR2 @ hex. FMC_BWTR2 1b. ;
    : FMC_BWTR3. cr ." FMC_BWTR3.  RW   $" FMC_BWTR3 @ hex. FMC_BWTR3 1b. ;
    : FMC_BWTR4. cr ." FMC_BWTR4.  RW   $" FMC_BWTR4 @ hex. FMC_BWTR4 1b. ;
    : FMC_SDCR1. cr ." FMC_SDCR1.  RW   $" FMC_SDCR1 @ hex. FMC_SDCR1 1b. ;
    : FMC_SDCR2. cr ." FMC_SDCR2.  RW   $" FMC_SDCR2 @ hex. FMC_SDCR2 1b. ;
    : FMC_SDTR1. cr ." FMC_SDTR1.  RW   $" FMC_SDTR1 @ hex. FMC_SDTR1 1b. ;
    : FMC_SDTR2. cr ." FMC_SDTR2.  RW   $" FMC_SDTR2 @ hex. FMC_SDTR2 1b. ;
    : FMC_SDCMR. cr ." FMC_SDCMR.   $" FMC_SDCMR @ hex. FMC_SDCMR 1b. ;
    : FMC_SDRTR. cr ." FMC_SDRTR.   $" FMC_SDRTR @ hex. FMC_SDRTR 1b. ;
    : FMC_SDSR. cr ." FMC_SDSR.  RO   $" FMC_SDSR @ hex. FMC_SDSR 1b. ;
    : FMC.
      FMC_BCR1.
      FMC_BTR1.
      FMC_BCR2.
      FMC_BTR2.
      FMC_BCR3.
      FMC_BTR3.
      FMC_BCR4.
      FMC_BTR4.
      FMC_PCR.
      FMC_SR.
      FMC_PMEM.
      FMC_PATT.
      FMC_ECCR.
      FMC_BWTR1.
      FMC_BWTR2.
      FMC_BWTR3.
      FMC_BWTR4.
      FMC_SDCR1.
      FMC_SDCR2.
      FMC_SDTR1.
      FMC_SDTR2.
      FMC_SDCMR.
      FMC_SDRTR.
      FMC_SDSR.
    ;
  [then]

  execute-defined? use-DBG [if]
    $E0042000 constant DBG ( Debug support ) 
    DBG $0 + constant DBG_DBGMCU_IDCODE ( read-only )  \ IDCODE
    DBG $4 + constant DBG_DBGMCU_CR ( read-write )  \ Control Register
    DBG $8 + constant DBG_DBGMCU_APB1_FZ ( read-write )  \ Debug MCU APB1 Freeze registe
    DBG $C + constant DBG_DBGMCU_APB2_FZ ( read-write )  \ Debug MCU APB2 Freeze registe
    : DBG_DBGMCU_IDCODE. cr ." DBG_DBGMCU_IDCODE.  RO   $" DBG_DBGMCU_IDCODE @ hex. DBG_DBGMCU_IDCODE 1b. ;
    : DBG_DBGMCU_CR. cr ." DBG_DBGMCU_CR.  RW   $" DBG_DBGMCU_CR @ hex. DBG_DBGMCU_CR 1b. ;
    : DBG_DBGMCU_APB1_FZ. cr ." DBG_DBGMCU_APB1_FZ.  RW   $" DBG_DBGMCU_APB1_FZ @ hex. DBG_DBGMCU_APB1_FZ 1b. ;
    : DBG_DBGMCU_APB2_FZ. cr ." DBG_DBGMCU_APB2_FZ.  RW   $" DBG_DBGMCU_APB2_FZ @ hex. DBG_DBGMCU_APB2_FZ 1b. ;
    : DBG.
      DBG_DBGMCU_IDCODE.
      DBG_DBGMCU_CR.
      DBG_DBGMCU_APB1_FZ.
      DBG_DBGMCU_APB2_FZ.
    ;
  [then]

  execute-defined? use-DMA2 [if]
    $40026400 constant DMA2 ( DMA controller ) 
    DMA2 $0 + constant DMA2_LISR ( read-only )  \ low interrupt status register
    DMA2 $4 + constant DMA2_HISR ( read-only )  \ high interrupt status register
    DMA2 $8 + constant DMA2_LIFCR ( read-write )  \ low interrupt flag clear register
    DMA2 $C + constant DMA2_HIFCR ( read-write )  \ high interrupt flag clear register
    DMA2 $10 + constant DMA2_S0CR ( read-write )  \ stream x configuration register
    DMA2 $14 + constant DMA2_S0NDTR ( read-write )  \ stream x number of data register
    DMA2 $18 + constant DMA2_S0PAR ( read-write )  \ stream x peripheral address register
    DMA2 $1C + constant DMA2_S0M0AR ( read-write )  \ stream x memory 0 address register
    DMA2 $20 + constant DMA2_S0M1AR ( read-write )  \ stream x memory 1 address register
    DMA2 $24 + constant DMA2_S0FCR (  )  \ stream x FIFO control register
    DMA2 $28 + constant DMA2_S1CR ( read-write )  \ stream x configuration register
    DMA2 $2C + constant DMA2_S1NDTR ( read-write )  \ stream x number of data register
    DMA2 $30 + constant DMA2_S1PAR ( read-write )  \ stream x peripheral address register
    DMA2 $34 + constant DMA2_S1M0AR ( read-write )  \ stream x memory 0 address register
    DMA2 $38 + constant DMA2_S1M1AR ( read-write )  \ stream x memory 1 address register
    DMA2 $3C + constant DMA2_S1FCR (  )  \ stream x FIFO control register
    DMA2 $40 + constant DMA2_S2CR ( read-write )  \ stream x configuration register
    DMA2 $44 + constant DMA2_S2NDTR ( read-write )  \ stream x number of data register
    DMA2 $48 + constant DMA2_S2PAR ( read-write )  \ stream x peripheral address register
    DMA2 $4C + constant DMA2_S2M0AR ( read-write )  \ stream x memory 0 address register
    DMA2 $50 + constant DMA2_S2M1AR ( read-write )  \ stream x memory 1 address register
    DMA2 $54 + constant DMA2_S2FCR (  )  \ stream x FIFO control register
    DMA2 $58 + constant DMA2_S3CR ( read-write )  \ stream x configuration register
    DMA2 $5C + constant DMA2_S3NDTR ( read-write )  \ stream x number of data register
    DMA2 $60 + constant DMA2_S3PAR ( read-write )  \ stream x peripheral address register
    DMA2 $64 + constant DMA2_S3M0AR ( read-write )  \ stream x memory 0 address register
    DMA2 $68 + constant DMA2_S3M1AR ( read-write )  \ stream x memory 1 address register
    DMA2 $6C + constant DMA2_S3FCR (  )  \ stream x FIFO control register
    DMA2 $70 + constant DMA2_S4CR ( read-write )  \ stream x configuration register
    DMA2 $74 + constant DMA2_S4NDTR ( read-write )  \ stream x number of data register
    DMA2 $78 + constant DMA2_S4PAR ( read-write )  \ stream x peripheral address register
    DMA2 $7C + constant DMA2_S4M0AR ( read-write )  \ stream x memory 0 address register
    DMA2 $80 + constant DMA2_S4M1AR ( read-write )  \ stream x memory 1 address register
    DMA2 $84 + constant DMA2_S4FCR (  )  \ stream x FIFO control register
    DMA2 $88 + constant DMA2_S5CR ( read-write )  \ stream x configuration register
    DMA2 $8C + constant DMA2_S5NDTR ( read-write )  \ stream x number of data register
    DMA2 $90 + constant DMA2_S5PAR ( read-write )  \ stream x peripheral address register
    DMA2 $94 + constant DMA2_S5M0AR ( read-write )  \ stream x memory 0 address register
    DMA2 $98 + constant DMA2_S5M1AR ( read-write )  \ stream x memory 1 address register
    DMA2 $9C + constant DMA2_S5FCR (  )  \ stream x FIFO control register
    DMA2 $A0 + constant DMA2_S6CR ( read-write )  \ stream x configuration register
    DMA2 $A4 + constant DMA2_S6NDTR ( read-write )  \ stream x number of data register
    DMA2 $A8 + constant DMA2_S6PAR ( read-write )  \ stream x peripheral address register
    DMA2 $AC + constant DMA2_S6M0AR ( read-write )  \ stream x memory 0 address register
    DMA2 $B0 + constant DMA2_S6M1AR ( read-write )  \ stream x memory 1 address register
    DMA2 $B4 + constant DMA2_S6FCR (  )  \ stream x FIFO control register
    DMA2 $B8 + constant DMA2_S7CR ( read-write )  \ stream x configuration register
    DMA2 $BC + constant DMA2_S7NDTR ( read-write )  \ stream x number of data register
    DMA2 $C0 + constant DMA2_S7PAR ( read-write )  \ stream x peripheral address register
    DMA2 $C4 + constant DMA2_S7M0AR ( read-write )  \ stream x memory 0 address register
    DMA2 $C8 + constant DMA2_S7M1AR ( read-write )  \ stream x memory 1 address register
    DMA2 $CC + constant DMA2_S7FCR (  )  \ stream x FIFO control register
    : DMA2_LISR. cr ." DMA2_LISR.  RO   $" DMA2_LISR @ hex. DMA2_LISR 1b. ;
    : DMA2_HISR. cr ." DMA2_HISR.  RO   $" DMA2_HISR @ hex. DMA2_HISR 1b. ;
    : DMA2_LIFCR. cr ." DMA2_LIFCR.  RW   $" DMA2_LIFCR @ hex. DMA2_LIFCR 1b. ;
    : DMA2_HIFCR. cr ." DMA2_HIFCR.  RW   $" DMA2_HIFCR @ hex. DMA2_HIFCR 1b. ;
    : DMA2_S0CR. cr ." DMA2_S0CR.  RW   $" DMA2_S0CR @ hex. DMA2_S0CR 1b. ;
    : DMA2_S0NDTR. cr ." DMA2_S0NDTR.  RW   $" DMA2_S0NDTR @ hex. DMA2_S0NDTR 1b. ;
    : DMA2_S0PAR. cr ." DMA2_S0PAR.  RW   $" DMA2_S0PAR @ hex. DMA2_S0PAR 1b. ;
    : DMA2_S0M0AR. cr ." DMA2_S0M0AR.  RW   $" DMA2_S0M0AR @ hex. DMA2_S0M0AR 1b. ;
    : DMA2_S0M1AR. cr ." DMA2_S0M1AR.  RW   $" DMA2_S0M1AR @ hex. DMA2_S0M1AR 1b. ;
    : DMA2_S0FCR. cr ." DMA2_S0FCR.   $" DMA2_S0FCR @ hex. DMA2_S0FCR 1b. ;
    : DMA2_S1CR. cr ." DMA2_S1CR.  RW   $" DMA2_S1CR @ hex. DMA2_S1CR 1b. ;
    : DMA2_S1NDTR. cr ." DMA2_S1NDTR.  RW   $" DMA2_S1NDTR @ hex. DMA2_S1NDTR 1b. ;
    : DMA2_S1PAR. cr ." DMA2_S1PAR.  RW   $" DMA2_S1PAR @ hex. DMA2_S1PAR 1b. ;
    : DMA2_S1M0AR. cr ." DMA2_S1M0AR.  RW   $" DMA2_S1M0AR @ hex. DMA2_S1M0AR 1b. ;
    : DMA2_S1M1AR. cr ." DMA2_S1M1AR.  RW   $" DMA2_S1M1AR @ hex. DMA2_S1M1AR 1b. ;
    : DMA2_S1FCR. cr ." DMA2_S1FCR.   $" DMA2_S1FCR @ hex. DMA2_S1FCR 1b. ;
    : DMA2_S2CR. cr ." DMA2_S2CR.  RW   $" DMA2_S2CR @ hex. DMA2_S2CR 1b. ;
    : DMA2_S2NDTR. cr ." DMA2_S2NDTR.  RW   $" DMA2_S2NDTR @ hex. DMA2_S2NDTR 1b. ;
    : DMA2_S2PAR. cr ." DMA2_S2PAR.  RW   $" DMA2_S2PAR @ hex. DMA2_S2PAR 1b. ;
    : DMA2_S2M0AR. cr ." DMA2_S2M0AR.  RW   $" DMA2_S2M0AR @ hex. DMA2_S2M0AR 1b. ;
    : DMA2_S2M1AR. cr ." DMA2_S2M1AR.  RW   $" DMA2_S2M1AR @ hex. DMA2_S2M1AR 1b. ;
    : DMA2_S2FCR. cr ." DMA2_S2FCR.   $" DMA2_S2FCR @ hex. DMA2_S2FCR 1b. ;
    : DMA2_S3CR. cr ." DMA2_S3CR.  RW   $" DMA2_S3CR @ hex. DMA2_S3CR 1b. ;
    : DMA2_S3NDTR. cr ." DMA2_S3NDTR.  RW   $" DMA2_S3NDTR @ hex. DMA2_S3NDTR 1b. ;
    : DMA2_S3PAR. cr ." DMA2_S3PAR.  RW   $" DMA2_S3PAR @ hex. DMA2_S3PAR 1b. ;
    : DMA2_S3M0AR. cr ." DMA2_S3M0AR.  RW   $" DMA2_S3M0AR @ hex. DMA2_S3M0AR 1b. ;
    : DMA2_S3M1AR. cr ." DMA2_S3M1AR.  RW   $" DMA2_S3M1AR @ hex. DMA2_S3M1AR 1b. ;
    : DMA2_S3FCR. cr ." DMA2_S3FCR.   $" DMA2_S3FCR @ hex. DMA2_S3FCR 1b. ;
    : DMA2_S4CR. cr ." DMA2_S4CR.  RW   $" DMA2_S4CR @ hex. DMA2_S4CR 1b. ;
    : DMA2_S4NDTR. cr ." DMA2_S4NDTR.  RW   $" DMA2_S4NDTR @ hex. DMA2_S4NDTR 1b. ;
    : DMA2_S4PAR. cr ." DMA2_S4PAR.  RW   $" DMA2_S4PAR @ hex. DMA2_S4PAR 1b. ;
    : DMA2_S4M0AR. cr ." DMA2_S4M0AR.  RW   $" DMA2_S4M0AR @ hex. DMA2_S4M0AR 1b. ;
    : DMA2_S4M1AR. cr ." DMA2_S4M1AR.  RW   $" DMA2_S4M1AR @ hex. DMA2_S4M1AR 1b. ;
    : DMA2_S4FCR. cr ." DMA2_S4FCR.   $" DMA2_S4FCR @ hex. DMA2_S4FCR 1b. ;
    : DMA2_S5CR. cr ." DMA2_S5CR.  RW   $" DMA2_S5CR @ hex. DMA2_S5CR 1b. ;
    : DMA2_S5NDTR. cr ." DMA2_S5NDTR.  RW   $" DMA2_S5NDTR @ hex. DMA2_S5NDTR 1b. ;
    : DMA2_S5PAR. cr ." DMA2_S5PAR.  RW   $" DMA2_S5PAR @ hex. DMA2_S5PAR 1b. ;
    : DMA2_S5M0AR. cr ." DMA2_S5M0AR.  RW   $" DMA2_S5M0AR @ hex. DMA2_S5M0AR 1b. ;
    : DMA2_S5M1AR. cr ." DMA2_S5M1AR.  RW   $" DMA2_S5M1AR @ hex. DMA2_S5M1AR 1b. ;
    : DMA2_S5FCR. cr ." DMA2_S5FCR.   $" DMA2_S5FCR @ hex. DMA2_S5FCR 1b. ;
    : DMA2_S6CR. cr ." DMA2_S6CR.  RW   $" DMA2_S6CR @ hex. DMA2_S6CR 1b. ;
    : DMA2_S6NDTR. cr ." DMA2_S6NDTR.  RW   $" DMA2_S6NDTR @ hex. DMA2_S6NDTR 1b. ;
    : DMA2_S6PAR. cr ." DMA2_S6PAR.  RW   $" DMA2_S6PAR @ hex. DMA2_S6PAR 1b. ;
    : DMA2_S6M0AR. cr ." DMA2_S6M0AR.  RW   $" DMA2_S6M0AR @ hex. DMA2_S6M0AR 1b. ;
    : DMA2_S6M1AR. cr ." DMA2_S6M1AR.  RW   $" DMA2_S6M1AR @ hex. DMA2_S6M1AR 1b. ;
    : DMA2_S6FCR. cr ." DMA2_S6FCR.   $" DMA2_S6FCR @ hex. DMA2_S6FCR 1b. ;
    : DMA2_S7CR. cr ." DMA2_S7CR.  RW   $" DMA2_S7CR @ hex. DMA2_S7CR 1b. ;
    : DMA2_S7NDTR. cr ." DMA2_S7NDTR.  RW   $" DMA2_S7NDTR @ hex. DMA2_S7NDTR 1b. ;
    : DMA2_S7PAR. cr ." DMA2_S7PAR.  RW   $" DMA2_S7PAR @ hex. DMA2_S7PAR 1b. ;
    : DMA2_S7M0AR. cr ." DMA2_S7M0AR.  RW   $" DMA2_S7M0AR @ hex. DMA2_S7M0AR 1b. ;
    : DMA2_S7M1AR. cr ." DMA2_S7M1AR.  RW   $" DMA2_S7M1AR @ hex. DMA2_S7M1AR 1b. ;
    : DMA2_S7FCR. cr ." DMA2_S7FCR.   $" DMA2_S7FCR @ hex. DMA2_S7FCR 1b. ;
    : DMA2.
      DMA2_LISR.
      DMA2_HISR.
      DMA2_LIFCR.
      DMA2_HIFCR.
      DMA2_S0CR.
      DMA2_S0NDTR.
      DMA2_S0PAR.
      DMA2_S0M0AR.
      DMA2_S0M1AR.
      DMA2_S0FCR.
      DMA2_S1CR.
      DMA2_S1NDTR.
      DMA2_S1PAR.
      DMA2_S1M0AR.
      DMA2_S1M1AR.
      DMA2_S1FCR.
      DMA2_S2CR.
      DMA2_S2NDTR.
      DMA2_S2PAR.
      DMA2_S2M0AR.
      DMA2_S2M1AR.
      DMA2_S2FCR.
      DMA2_S3CR.
      DMA2_S3NDTR.
      DMA2_S3PAR.
      DMA2_S3M0AR.
      DMA2_S3M1AR.
      DMA2_S3FCR.
      DMA2_S4CR.
      DMA2_S4NDTR.
      DMA2_S4PAR.
      DMA2_S4M0AR.
      DMA2_S4M1AR.
      DMA2_S4FCR.
      DMA2_S5CR.
      DMA2_S5NDTR.
      DMA2_S5PAR.
      DMA2_S5M0AR.
      DMA2_S5M1AR.
      DMA2_S5FCR.
      DMA2_S6CR.
      DMA2_S6NDTR.
      DMA2_S6PAR.
      DMA2_S6M0AR.
      DMA2_S6M1AR.
      DMA2_S6FCR.
      DMA2_S7CR.
      DMA2_S7NDTR.
      DMA2_S7PAR.
      DMA2_S7M0AR.
      DMA2_S7M1AR.
      DMA2_S7FCR.
    ;
  [then]

  execute-defined? use-DMA1 [if]
    $40026000 constant DMA1 ( DMA controller ) 
    DMA1 $0 + constant DMA1_LISR ( read-only )  \ low interrupt status register
    DMA1 $4 + constant DMA1_HISR ( read-only )  \ high interrupt status register
    DMA1 $8 + constant DMA1_LIFCR ( read-write )  \ low interrupt flag clear register
    DMA1 $C + constant DMA1_HIFCR ( read-write )  \ high interrupt flag clear register
    DMA1 $10 + constant DMA1_S0CR ( read-write )  \ stream x configuration register
    DMA1 $14 + constant DMA1_S0NDTR ( read-write )  \ stream x number of data register
    DMA1 $18 + constant DMA1_S0PAR ( read-write )  \ stream x peripheral address register
    DMA1 $1C + constant DMA1_S0M0AR ( read-write )  \ stream x memory 0 address register
    DMA1 $20 + constant DMA1_S0M1AR ( read-write )  \ stream x memory 1 address register
    DMA1 $24 + constant DMA1_S0FCR (  )  \ stream x FIFO control register
    DMA1 $28 + constant DMA1_S1CR ( read-write )  \ stream x configuration register
    DMA1 $2C + constant DMA1_S1NDTR ( read-write )  \ stream x number of data register
    DMA1 $30 + constant DMA1_S1PAR ( read-write )  \ stream x peripheral address register
    DMA1 $34 + constant DMA1_S1M0AR ( read-write )  \ stream x memory 0 address register
    DMA1 $38 + constant DMA1_S1M1AR ( read-write )  \ stream x memory 1 address register
    DMA1 $3C + constant DMA1_S1FCR (  )  \ stream x FIFO control register
    DMA1 $40 + constant DMA1_S2CR ( read-write )  \ stream x configuration register
    DMA1 $44 + constant DMA1_S2NDTR ( read-write )  \ stream x number of data register
    DMA1 $48 + constant DMA1_S2PAR ( read-write )  \ stream x peripheral address register
    DMA1 $4C + constant DMA1_S2M0AR ( read-write )  \ stream x memory 0 address register
    DMA1 $50 + constant DMA1_S2M1AR ( read-write )  \ stream x memory 1 address register
    DMA1 $54 + constant DMA1_S2FCR (  )  \ stream x FIFO control register
    DMA1 $58 + constant DMA1_S3CR ( read-write )  \ stream x configuration register
    DMA1 $5C + constant DMA1_S3NDTR ( read-write )  \ stream x number of data register
    DMA1 $60 + constant DMA1_S3PAR ( read-write )  \ stream x peripheral address register
    DMA1 $64 + constant DMA1_S3M0AR ( read-write )  \ stream x memory 0 address register
    DMA1 $68 + constant DMA1_S3M1AR ( read-write )  \ stream x memory 1 address register
    DMA1 $6C + constant DMA1_S3FCR (  )  \ stream x FIFO control register
    DMA1 $70 + constant DMA1_S4CR ( read-write )  \ stream x configuration register
    DMA1 $74 + constant DMA1_S4NDTR ( read-write )  \ stream x number of data register
    DMA1 $78 + constant DMA1_S4PAR ( read-write )  \ stream x peripheral address register
    DMA1 $7C + constant DMA1_S4M0AR ( read-write )  \ stream x memory 0 address register
    DMA1 $80 + constant DMA1_S4M1AR ( read-write )  \ stream x memory 1 address register
    DMA1 $84 + constant DMA1_S4FCR (  )  \ stream x FIFO control register
    DMA1 $88 + constant DMA1_S5CR ( read-write )  \ stream x configuration register
    DMA1 $8C + constant DMA1_S5NDTR ( read-write )  \ stream x number of data register
    DMA1 $90 + constant DMA1_S5PAR ( read-write )  \ stream x peripheral address register
    DMA1 $94 + constant DMA1_S5M0AR ( read-write )  \ stream x memory 0 address register
    DMA1 $98 + constant DMA1_S5M1AR ( read-write )  \ stream x memory 1 address register
    DMA1 $9C + constant DMA1_S5FCR (  )  \ stream x FIFO control register
    DMA1 $A0 + constant DMA1_S6CR ( read-write )  \ stream x configuration register
    DMA1 $A4 + constant DMA1_S6NDTR ( read-write )  \ stream x number of data register
    DMA1 $A8 + constant DMA1_S6PAR ( read-write )  \ stream x peripheral address register
    DMA1 $AC + constant DMA1_S6M0AR ( read-write )  \ stream x memory 0 address register
    DMA1 $B0 + constant DMA1_S6M1AR ( read-write )  \ stream x memory 1 address register
    DMA1 $B4 + constant DMA1_S6FCR (  )  \ stream x FIFO control register
    DMA1 $B8 + constant DMA1_S7CR ( read-write )  \ stream x configuration register
    DMA1 $BC + constant DMA1_S7NDTR ( read-write )  \ stream x number of data register
    DMA1 $C0 + constant DMA1_S7PAR ( read-write )  \ stream x peripheral address register
    DMA1 $C4 + constant DMA1_S7M0AR ( read-write )  \ stream x memory 0 address register
    DMA1 $C8 + constant DMA1_S7M1AR ( read-write )  \ stream x memory 1 address register
    DMA1 $CC + constant DMA1_S7FCR (  )  \ stream x FIFO control register
    : DMA1_LISR. cr ." DMA1_LISR.  RO   $" DMA1_LISR @ hex. DMA1_LISR 1b. ;
    : DMA1_HISR. cr ." DMA1_HISR.  RO   $" DMA1_HISR @ hex. DMA1_HISR 1b. ;
    : DMA1_LIFCR. cr ." DMA1_LIFCR.  RW   $" DMA1_LIFCR @ hex. DMA1_LIFCR 1b. ;
    : DMA1_HIFCR. cr ." DMA1_HIFCR.  RW   $" DMA1_HIFCR @ hex. DMA1_HIFCR 1b. ;
    : DMA1_S0CR. cr ." DMA1_S0CR.  RW   $" DMA1_S0CR @ hex. DMA1_S0CR 1b. ;
    : DMA1_S0NDTR. cr ." DMA1_S0NDTR.  RW   $" DMA1_S0NDTR @ hex. DMA1_S0NDTR 1b. ;
    : DMA1_S0PAR. cr ." DMA1_S0PAR.  RW   $" DMA1_S0PAR @ hex. DMA1_S0PAR 1b. ;
    : DMA1_S0M0AR. cr ." DMA1_S0M0AR.  RW   $" DMA1_S0M0AR @ hex. DMA1_S0M0AR 1b. ;
    : DMA1_S0M1AR. cr ." DMA1_S0M1AR.  RW   $" DMA1_S0M1AR @ hex. DMA1_S0M1AR 1b. ;
    : DMA1_S0FCR. cr ." DMA1_S0FCR.   $" DMA1_S0FCR @ hex. DMA1_S0FCR 1b. ;
    : DMA1_S1CR. cr ." DMA1_S1CR.  RW   $" DMA1_S1CR @ hex. DMA1_S1CR 1b. ;
    : DMA1_S1NDTR. cr ." DMA1_S1NDTR.  RW   $" DMA1_S1NDTR @ hex. DMA1_S1NDTR 1b. ;
    : DMA1_S1PAR. cr ." DMA1_S1PAR.  RW   $" DMA1_S1PAR @ hex. DMA1_S1PAR 1b. ;
    : DMA1_S1M0AR. cr ." DMA1_S1M0AR.  RW   $" DMA1_S1M0AR @ hex. DMA1_S1M0AR 1b. ;
    : DMA1_S1M1AR. cr ." DMA1_S1M1AR.  RW   $" DMA1_S1M1AR @ hex. DMA1_S1M1AR 1b. ;
    : DMA1_S1FCR. cr ." DMA1_S1FCR.   $" DMA1_S1FCR @ hex. DMA1_S1FCR 1b. ;
    : DMA1_S2CR. cr ." DMA1_S2CR.  RW   $" DMA1_S2CR @ hex. DMA1_S2CR 1b. ;
    : DMA1_S2NDTR. cr ." DMA1_S2NDTR.  RW   $" DMA1_S2NDTR @ hex. DMA1_S2NDTR 1b. ;
    : DMA1_S2PAR. cr ." DMA1_S2PAR.  RW   $" DMA1_S2PAR @ hex. DMA1_S2PAR 1b. ;
    : DMA1_S2M0AR. cr ." DMA1_S2M0AR.  RW   $" DMA1_S2M0AR @ hex. DMA1_S2M0AR 1b. ;
    : DMA1_S2M1AR. cr ." DMA1_S2M1AR.  RW   $" DMA1_S2M1AR @ hex. DMA1_S2M1AR 1b. ;
    : DMA1_S2FCR. cr ." DMA1_S2FCR.   $" DMA1_S2FCR @ hex. DMA1_S2FCR 1b. ;
    : DMA1_S3CR. cr ." DMA1_S3CR.  RW   $" DMA1_S3CR @ hex. DMA1_S3CR 1b. ;
    : DMA1_S3NDTR. cr ." DMA1_S3NDTR.  RW   $" DMA1_S3NDTR @ hex. DMA1_S3NDTR 1b. ;
    : DMA1_S3PAR. cr ." DMA1_S3PAR.  RW   $" DMA1_S3PAR @ hex. DMA1_S3PAR 1b. ;
    : DMA1_S3M0AR. cr ." DMA1_S3M0AR.  RW   $" DMA1_S3M0AR @ hex. DMA1_S3M0AR 1b. ;
    : DMA1_S3M1AR. cr ." DMA1_S3M1AR.  RW   $" DMA1_S3M1AR @ hex. DMA1_S3M1AR 1b. ;
    : DMA1_S3FCR. cr ." DMA1_S3FCR.   $" DMA1_S3FCR @ hex. DMA1_S3FCR 1b. ;
    : DMA1_S4CR. cr ." DMA1_S4CR.  RW   $" DMA1_S4CR @ hex. DMA1_S4CR 1b. ;
    : DMA1_S4NDTR. cr ." DMA1_S4NDTR.  RW   $" DMA1_S4NDTR @ hex. DMA1_S4NDTR 1b. ;
    : DMA1_S4PAR. cr ." DMA1_S4PAR.  RW   $" DMA1_S4PAR @ hex. DMA1_S4PAR 1b. ;
    : DMA1_S4M0AR. cr ." DMA1_S4M0AR.  RW   $" DMA1_S4M0AR @ hex. DMA1_S4M0AR 1b. ;
    : DMA1_S4M1AR. cr ." DMA1_S4M1AR.  RW   $" DMA1_S4M1AR @ hex. DMA1_S4M1AR 1b. ;
    : DMA1_S4FCR. cr ." DMA1_S4FCR.   $" DMA1_S4FCR @ hex. DMA1_S4FCR 1b. ;
    : DMA1_S5CR. cr ." DMA1_S5CR.  RW   $" DMA1_S5CR @ hex. DMA1_S5CR 1b. ;
    : DMA1_S5NDTR. cr ." DMA1_S5NDTR.  RW   $" DMA1_S5NDTR @ hex. DMA1_S5NDTR 1b. ;
    : DMA1_S5PAR. cr ." DMA1_S5PAR.  RW   $" DMA1_S5PAR @ hex. DMA1_S5PAR 1b. ;
    : DMA1_S5M0AR. cr ." DMA1_S5M0AR.  RW   $" DMA1_S5M0AR @ hex. DMA1_S5M0AR 1b. ;
    : DMA1_S5M1AR. cr ." DMA1_S5M1AR.  RW   $" DMA1_S5M1AR @ hex. DMA1_S5M1AR 1b. ;
    : DMA1_S5FCR. cr ." DMA1_S5FCR.   $" DMA1_S5FCR @ hex. DMA1_S5FCR 1b. ;
    : DMA1_S6CR. cr ." DMA1_S6CR.  RW   $" DMA1_S6CR @ hex. DMA1_S6CR 1b. ;
    : DMA1_S6NDTR. cr ." DMA1_S6NDTR.  RW   $" DMA1_S6NDTR @ hex. DMA1_S6NDTR 1b. ;
    : DMA1_S6PAR. cr ." DMA1_S6PAR.  RW   $" DMA1_S6PAR @ hex. DMA1_S6PAR 1b. ;
    : DMA1_S6M0AR. cr ." DMA1_S6M0AR.  RW   $" DMA1_S6M0AR @ hex. DMA1_S6M0AR 1b. ;
    : DMA1_S6M1AR. cr ." DMA1_S6M1AR.  RW   $" DMA1_S6M1AR @ hex. DMA1_S6M1AR 1b. ;
    : DMA1_S6FCR. cr ." DMA1_S6FCR.   $" DMA1_S6FCR @ hex. DMA1_S6FCR 1b. ;
    : DMA1_S7CR. cr ." DMA1_S7CR.  RW   $" DMA1_S7CR @ hex. DMA1_S7CR 1b. ;
    : DMA1_S7NDTR. cr ." DMA1_S7NDTR.  RW   $" DMA1_S7NDTR @ hex. DMA1_S7NDTR 1b. ;
    : DMA1_S7PAR. cr ." DMA1_S7PAR.  RW   $" DMA1_S7PAR @ hex. DMA1_S7PAR 1b. ;
    : DMA1_S7M0AR. cr ." DMA1_S7M0AR.  RW   $" DMA1_S7M0AR @ hex. DMA1_S7M0AR 1b. ;
    : DMA1_S7M1AR. cr ." DMA1_S7M1AR.  RW   $" DMA1_S7M1AR @ hex. DMA1_S7M1AR 1b. ;
    : DMA1_S7FCR. cr ." DMA1_S7FCR.   $" DMA1_S7FCR @ hex. DMA1_S7FCR 1b. ;
    : DMA1.
      DMA1_LISR.
      DMA1_HISR.
      DMA1_LIFCR.
      DMA1_HIFCR.
      DMA1_S0CR.
      DMA1_S0NDTR.
      DMA1_S0PAR.
      DMA1_S0M0AR.
      DMA1_S0M1AR.
      DMA1_S0FCR.
      DMA1_S1CR.
      DMA1_S1NDTR.
      DMA1_S1PAR.
      DMA1_S1M0AR.
      DMA1_S1M1AR.
      DMA1_S1FCR.
      DMA1_S2CR.
      DMA1_S2NDTR.
      DMA1_S2PAR.
      DMA1_S2M0AR.
      DMA1_S2M1AR.
      DMA1_S2FCR.
      DMA1_S3CR.
      DMA1_S3NDTR.
      DMA1_S3PAR.
      DMA1_S3M0AR.
      DMA1_S3M1AR.
      DMA1_S3FCR.
      DMA1_S4CR.
      DMA1_S4NDTR.
      DMA1_S4PAR.
      DMA1_S4M0AR.
      DMA1_S4M1AR.
      DMA1_S4FCR.
      DMA1_S5CR.
      DMA1_S5NDTR.
      DMA1_S5PAR.
      DMA1_S5M0AR.
      DMA1_S5M1AR.
      DMA1_S5FCR.
      DMA1_S6CR.
      DMA1_S6NDTR.
      DMA1_S6PAR.
      DMA1_S6M0AR.
      DMA1_S6M1AR.
      DMA1_S6FCR.
      DMA1_S7CR.
      DMA1_S7NDTR.
      DMA1_S7PAR.
      DMA1_S7M0AR.
      DMA1_S7M1AR.
      DMA1_S7FCR.
    ;
  [then]

  execute-defined? use-RCC [if]
    $40023800 constant RCC ( Reset and clock control ) 
    RCC $0 + constant RCC_CR (  )  \ clock control register
    RCC $4 + constant RCC_PLLCFGR ( read-write )  \ PLL configuration register
    RCC $8 + constant RCC_CFGR (  )  \ clock configuration register
    RCC $C + constant RCC_CIR (  )  \ clock interrupt register
    RCC $10 + constant RCC_AHB1RSTR ( read-write )  \ AHB1 peripheral reset register
    RCC $14 + constant RCC_AHB2RSTR ( read-write )  \ AHB2 peripheral reset register
    RCC $18 + constant RCC_AHB3RSTR ( read-write )  \ AHB3 peripheral reset register
    RCC $20 + constant RCC_APB1RSTR ( read-write )  \ APB1 peripheral reset register
    RCC $24 + constant RCC_APB2RSTR ( read-write )  \ APB2 peripheral reset register
    RCC $30 + constant RCC_AHB1ENR ( read-write )  \ AHB1 peripheral clock register
    RCC $34 + constant RCC_AHB2ENR ( read-write )  \ AHB2 peripheral clock enable register
    RCC $38 + constant RCC_AHB3ENR ( read-write )  \ AHB3 peripheral clock enable register
    RCC $40 + constant RCC_APB1ENR ( read-write )  \ APB1 peripheral clock enable register
    RCC $44 + constant RCC_APB2ENR ( read-write )  \ APB2 peripheral clock enable register
    RCC $50 + constant RCC_AHB1LPENR ( read-write )  \ AHB1 peripheral clock enable in low power mode register
    RCC $54 + constant RCC_AHB2LPENR ( read-write )  \ AHB2 peripheral clock enable in low power mode register
    RCC $58 + constant RCC_AHB3LPENR ( read-write )  \ AHB3 peripheral clock enable in low power mode register
    RCC $60 + constant RCC_APB1LPENR ( read-write )  \ APB1 peripheral clock enable in low power mode register
    RCC $64 + constant RCC_APB2LPENR ( read-write )  \ APB2 peripheral clock enabled in low power mode register
    RCC $70 + constant RCC_BDCR (  )  \ Backup domain control register
    RCC $74 + constant RCC_CSR (  )  \ clock control & status register
    RCC $80 + constant RCC_SSCGR ( read-write )  \ spread spectrum clock generation register
    RCC $84 + constant RCC_PLLI2SCFGR ( read-write )  \ PLLI2S configuration register
    RCC $88 + constant RCC_PLLSAICFGR ( read-write )  \ PLL configuration register
    RCC $8C + constant RCC_DKCFGR1 ( read-write )  \ dedicated clocks configuration register
    RCC $90 + constant RCC_DKCFGR2 ( read-write )  \ dedicated clocks configuration register
    : RCC_CR. cr ." RCC_CR.   $" RCC_CR @ hex. RCC_CR 1b. ;
    : RCC_PLLCFGR. cr ." RCC_PLLCFGR.  RW   $" RCC_PLLCFGR @ hex. RCC_PLLCFGR 1b. ;
    : RCC_CFGR. cr ." RCC_CFGR.   $" RCC_CFGR @ hex. RCC_CFGR 1b. ;
    : RCC_CIR. cr ." RCC_CIR.   $" RCC_CIR @ hex. RCC_CIR 1b. ;
    : RCC_AHB1RSTR. cr ." RCC_AHB1RSTR.  RW   $" RCC_AHB1RSTR @ hex. RCC_AHB1RSTR 1b. ;
    : RCC_AHB2RSTR. cr ." RCC_AHB2RSTR.  RW   $" RCC_AHB2RSTR @ hex. RCC_AHB2RSTR 1b. ;
    : RCC_AHB3RSTR. cr ." RCC_AHB3RSTR.  RW   $" RCC_AHB3RSTR @ hex. RCC_AHB3RSTR 1b. ;
    : RCC_APB1RSTR. cr ." RCC_APB1RSTR.  RW   $" RCC_APB1RSTR @ hex. RCC_APB1RSTR 1b. ;
    : RCC_APB2RSTR. cr ." RCC_APB2RSTR.  RW   $" RCC_APB2RSTR @ hex. RCC_APB2RSTR 1b. ;
    : RCC_AHB1ENR. cr ." RCC_AHB1ENR.  RW   $" RCC_AHB1ENR @ hex. RCC_AHB1ENR 1b. ;
    : RCC_AHB2ENR. cr ." RCC_AHB2ENR.  RW   $" RCC_AHB2ENR @ hex. RCC_AHB2ENR 1b. ;
    : RCC_AHB3ENR. cr ." RCC_AHB3ENR.  RW   $" RCC_AHB3ENR @ hex. RCC_AHB3ENR 1b. ;
    : RCC_APB1ENR. cr ." RCC_APB1ENR.  RW   $" RCC_APB1ENR @ hex. RCC_APB1ENR 1b. ;
    : RCC_APB2ENR. cr ." RCC_APB2ENR.  RW   $" RCC_APB2ENR @ hex. RCC_APB2ENR 1b. ;
    : RCC_AHB1LPENR. cr ." RCC_AHB1LPENR.  RW   $" RCC_AHB1LPENR @ hex. RCC_AHB1LPENR 1b. ;
    : RCC_AHB2LPENR. cr ." RCC_AHB2LPENR.  RW   $" RCC_AHB2LPENR @ hex. RCC_AHB2LPENR 1b. ;
    : RCC_AHB3LPENR. cr ." RCC_AHB3LPENR.  RW   $" RCC_AHB3LPENR @ hex. RCC_AHB3LPENR 1b. ;
    : RCC_APB1LPENR. cr ." RCC_APB1LPENR.  RW   $" RCC_APB1LPENR @ hex. RCC_APB1LPENR 1b. ;
    : RCC_APB2LPENR. cr ." RCC_APB2LPENR.  RW   $" RCC_APB2LPENR @ hex. RCC_APB2LPENR 1b. ;
    : RCC_BDCR. cr ." RCC_BDCR.   $" RCC_BDCR @ hex. RCC_BDCR 1b. ;
    : RCC_CSR. cr ." RCC_CSR.   $" RCC_CSR @ hex. RCC_CSR 1b. ;
    : RCC_SSCGR. cr ." RCC_SSCGR.  RW   $" RCC_SSCGR @ hex. RCC_SSCGR 1b. ;
    : RCC_PLLI2SCFGR. cr ." RCC_PLLI2SCFGR.  RW   $" RCC_PLLI2SCFGR @ hex. RCC_PLLI2SCFGR 1b. ;
    : RCC_PLLSAICFGR. cr ." RCC_PLLSAICFGR.  RW   $" RCC_PLLSAICFGR @ hex. RCC_PLLSAICFGR 1b. ;
    : RCC_DKCFGR1. cr ." RCC_DKCFGR1.  RW   $" RCC_DKCFGR1 @ hex. RCC_DKCFGR1 1b. ;
    : RCC_DKCFGR2. cr ." RCC_DKCFGR2.  RW   $" RCC_DKCFGR2 @ hex. RCC_DKCFGR2 1b. ;
    : RCC.
      RCC_CR.
      RCC_PLLCFGR.
      RCC_CFGR.
      RCC_CIR.
      RCC_AHB1RSTR.
      RCC_AHB2RSTR.
      RCC_AHB3RSTR.
      RCC_APB1RSTR.
      RCC_APB2RSTR.
      RCC_AHB1ENR.
      RCC_AHB2ENR.
      RCC_AHB3ENR.
      RCC_APB1ENR.
      RCC_APB2ENR.
      RCC_AHB1LPENR.
      RCC_AHB2LPENR.
      RCC_AHB3LPENR.
      RCC_APB1LPENR.
      RCC_APB2LPENR.
      RCC_BDCR.
      RCC_CSR.
      RCC_SSCGR.
      RCC_PLLI2SCFGR.
      RCC_PLLSAICFGR.
      RCC_DKCFGR1.
      RCC_DKCFGR2.
    ;
  [then]

  execute-defined? use-GPIOD [if]
    $40020C00 constant GPIOD ( General-purpose I/Os ) 
    GPIOD $0 + constant GPIOD_MODER ( read-write )  \ GPIO port mode register
    GPIOD $4 + constant GPIOD_OTYPER ( read-write )  \ GPIO port output type register
    GPIOD $8 + constant GPIOD_GPIOB_OSPEEDR ( read-write )  \ GPIO port output speed register
    GPIOD $C + constant GPIOD_PUPDR ( read-write )  \ GPIO port pull-up/pull-down register
    GPIOD $10 + constant GPIOD_IDR ( read-only )  \ GPIO port input data register
    GPIOD $14 + constant GPIOD_ODR ( read-write )  \ GPIO port output data register
    GPIOD $18 + constant GPIOD_BSRR ( write-only )  \ GPIO port bit set/reset register
    GPIOD $1C + constant GPIOD_LCKR ( read-write )  \ GPIO port configuration lock register
    GPIOD $20 + constant GPIOD_AFRL ( read-write )  \ GPIO alternate function lowregister
    GPIOD $24 + constant GPIOD_AFRH ( read-write )  \ GPIO alternate function high register
    GPIOD $28 + constant GPIOD_BRR ( read-write )  \ GPIO port bit reset register
    : GPIOD_MODER. cr ." GPIOD_MODER.  RW   $" GPIOD_MODER @ hex. GPIOD_MODER 1b. ;
    : GPIOD_OTYPER. cr ." GPIOD_OTYPER.  RW   $" GPIOD_OTYPER @ hex. GPIOD_OTYPER 1b. ;
    : GPIOD_GPIOB_OSPEEDR. cr ." GPIOD_GPIOB_OSPEEDR.  RW   $" GPIOD_GPIOB_OSPEEDR @ hex. GPIOD_GPIOB_OSPEEDR 1b. ;
    : GPIOD_PUPDR. cr ." GPIOD_PUPDR.  RW   $" GPIOD_PUPDR @ hex. GPIOD_PUPDR 1b. ;
    : GPIOD_IDR. cr ." GPIOD_IDR.  RO   $" GPIOD_IDR @ hex. GPIOD_IDR 1b. ;
    : GPIOD_ODR. cr ." GPIOD_ODR.  RW   $" GPIOD_ODR @ hex. GPIOD_ODR 1b. ;
    : GPIOD_BSRR. cr ." GPIOD_BSRR " WRITEONLY ; 
    : GPIOD_LCKR. cr ." GPIOD_LCKR.  RW   $" GPIOD_LCKR @ hex. GPIOD_LCKR 1b. ;
    : GPIOD_AFRL. cr ." GPIOD_AFRL.  RW   $" GPIOD_AFRL @ hex. GPIOD_AFRL 1b. ;
    : GPIOD_AFRH. cr ." GPIOD_AFRH.  RW   $" GPIOD_AFRH @ hex. GPIOD_AFRH 1b. ;
    : GPIOD_BRR. cr ." GPIOD_BRR.  RW   $" GPIOD_BRR @ hex. GPIOD_BRR 1b. ;
    : GPIOD.
      GPIOD_MODER.
      GPIOD_OTYPER.
      GPIOD_GPIOB_OSPEEDR.
      GPIOD_PUPDR.
      GPIOD_IDR.
      GPIOD_ODR.
      GPIOD_BSRR.
      GPIOD_LCKR.
      GPIOD_AFRL.
      GPIOD_AFRH.
      GPIOD_BRR.
    ;
  [then]

  execute-defined? use-GPIOC [if]
    $40020800 constant GPIOC ( General-purpose I/Os ) 
    GPIOC $0 + constant GPIOC_MODER ( read-write )  \ GPIO port mode register
    GPIOC $4 + constant GPIOC_OTYPER ( read-write )  \ GPIO port output type register
    GPIOC $8 + constant GPIOC_GPIOB_OSPEEDR ( read-write )  \ GPIO port output speed register
    GPIOC $C + constant GPIOC_PUPDR ( read-write )  \ GPIO port pull-up/pull-down register
    GPIOC $10 + constant GPIOC_IDR ( read-only )  \ GPIO port input data register
    GPIOC $14 + constant GPIOC_ODR ( read-write )  \ GPIO port output data register
    GPIOC $18 + constant GPIOC_BSRR ( write-only )  \ GPIO port bit set/reset register
    GPIOC $1C + constant GPIOC_LCKR ( read-write )  \ GPIO port configuration lock register
    GPIOC $20 + constant GPIOC_AFRL ( read-write )  \ GPIO alternate function lowregister
    GPIOC $24 + constant GPIOC_AFRH ( read-write )  \ GPIO alternate function high register
    GPIOC $28 + constant GPIOC_BRR ( read-write )  \ GPIO port bit reset register
    : GPIOC_MODER. cr ." GPIOC_MODER.  RW   $" GPIOC_MODER @ hex. GPIOC_MODER 1b. ;
    : GPIOC_OTYPER. cr ." GPIOC_OTYPER.  RW   $" GPIOC_OTYPER @ hex. GPIOC_OTYPER 1b. ;
    : GPIOC_GPIOB_OSPEEDR. cr ." GPIOC_GPIOB_OSPEEDR.  RW   $" GPIOC_GPIOB_OSPEEDR @ hex. GPIOC_GPIOB_OSPEEDR 1b. ;
    : GPIOC_PUPDR. cr ." GPIOC_PUPDR.  RW   $" GPIOC_PUPDR @ hex. GPIOC_PUPDR 1b. ;
    : GPIOC_IDR. cr ." GPIOC_IDR.  RO   $" GPIOC_IDR @ hex. GPIOC_IDR 1b. ;
    : GPIOC_ODR. cr ." GPIOC_ODR.  RW   $" GPIOC_ODR @ hex. GPIOC_ODR 1b. ;
    : GPIOC_BSRR. cr ." GPIOC_BSRR " WRITEONLY ; 
    : GPIOC_LCKR. cr ." GPIOC_LCKR.  RW   $" GPIOC_LCKR @ hex. GPIOC_LCKR 1b. ;
    : GPIOC_AFRL. cr ." GPIOC_AFRL.  RW   $" GPIOC_AFRL @ hex. GPIOC_AFRL 1b. ;
    : GPIOC_AFRH. cr ." GPIOC_AFRH.  RW   $" GPIOC_AFRH @ hex. GPIOC_AFRH 1b. ;
    : GPIOC_BRR. cr ." GPIOC_BRR.  RW   $" GPIOC_BRR @ hex. GPIOC_BRR 1b. ;
    : GPIOC.
      GPIOC_MODER.
      GPIOC_OTYPER.
      GPIOC_GPIOB_OSPEEDR.
      GPIOC_PUPDR.
      GPIOC_IDR.
      GPIOC_ODR.
      GPIOC_BSRR.
      GPIOC_LCKR.
      GPIOC_AFRL.
      GPIOC_AFRH.
      GPIOC_BRR.
    ;
  [then]

  execute-defined? use-GPIOK [if]
    $40022800 constant GPIOK ( General-purpose I/Os ) 
    GPIOK $0 + constant GPIOK_MODER ( read-write )  \ GPIO port mode register
    GPIOK $4 + constant GPIOK_OTYPER ( read-write )  \ GPIO port output type register
    GPIOK $8 + constant GPIOK_GPIOB_OSPEEDR ( read-write )  \ GPIO port output speed register
    GPIOK $C + constant GPIOK_PUPDR ( read-write )  \ GPIO port pull-up/pull-down register
    GPIOK $10 + constant GPIOK_IDR ( read-only )  \ GPIO port input data register
    GPIOK $14 + constant GPIOK_ODR ( read-write )  \ GPIO port output data register
    GPIOK $18 + constant GPIOK_BSRR ( write-only )  \ GPIO port bit set/reset register
    GPIOK $1C + constant GPIOK_LCKR ( read-write )  \ GPIO port configuration lock register
    GPIOK $20 + constant GPIOK_AFRL ( read-write )  \ GPIO alternate function lowregister
    GPIOK $24 + constant GPIOK_AFRH ( read-write )  \ GPIO alternate function high register
    GPIOK $28 + constant GPIOK_BRR ( read-write )  \ GPIO port bit reset register
    : GPIOK_MODER. cr ." GPIOK_MODER.  RW   $" GPIOK_MODER @ hex. GPIOK_MODER 1b. ;
    : GPIOK_OTYPER. cr ." GPIOK_OTYPER.  RW   $" GPIOK_OTYPER @ hex. GPIOK_OTYPER 1b. ;
    : GPIOK_GPIOB_OSPEEDR. cr ." GPIOK_GPIOB_OSPEEDR.  RW   $" GPIOK_GPIOB_OSPEEDR @ hex. GPIOK_GPIOB_OSPEEDR 1b. ;
    : GPIOK_PUPDR. cr ." GPIOK_PUPDR.  RW   $" GPIOK_PUPDR @ hex. GPIOK_PUPDR 1b. ;
    : GPIOK_IDR. cr ." GPIOK_IDR.  RO   $" GPIOK_IDR @ hex. GPIOK_IDR 1b. ;
    : GPIOK_ODR. cr ." GPIOK_ODR.  RW   $" GPIOK_ODR @ hex. GPIOK_ODR 1b. ;
    : GPIOK_BSRR. cr ." GPIOK_BSRR " WRITEONLY ; 
    : GPIOK_LCKR. cr ." GPIOK_LCKR.  RW   $" GPIOK_LCKR @ hex. GPIOK_LCKR 1b. ;
    : GPIOK_AFRL. cr ." GPIOK_AFRL.  RW   $" GPIOK_AFRL @ hex. GPIOK_AFRL 1b. ;
    : GPIOK_AFRH. cr ." GPIOK_AFRH.  RW   $" GPIOK_AFRH @ hex. GPIOK_AFRH 1b. ;
    : GPIOK_BRR. cr ." GPIOK_BRR.  RW   $" GPIOK_BRR @ hex. GPIOK_BRR 1b. ;
    : GPIOK.
      GPIOK_MODER.
      GPIOK_OTYPER.
      GPIOK_GPIOB_OSPEEDR.
      GPIOK_PUPDR.
      GPIOK_IDR.
      GPIOK_ODR.
      GPIOK_BSRR.
      GPIOK_LCKR.
      GPIOK_AFRL.
      GPIOK_AFRH.
      GPIOK_BRR.
    ;
  [then]

  execute-defined? use-GPIOJ [if]
    $40022400 constant GPIOJ ( General-purpose I/Os ) 
    GPIOJ $0 + constant GPIOJ_MODER ( read-write )  \ GPIO port mode register
    GPIOJ $4 + constant GPIOJ_OTYPER ( read-write )  \ GPIO port output type register
    GPIOJ $8 + constant GPIOJ_GPIOB_OSPEEDR ( read-write )  \ GPIO port output speed register
    GPIOJ $C + constant GPIOJ_PUPDR ( read-write )  \ GPIO port pull-up/pull-down register
    GPIOJ $10 + constant GPIOJ_IDR ( read-only )  \ GPIO port input data register
    GPIOJ $14 + constant GPIOJ_ODR ( read-write )  \ GPIO port output data register
    GPIOJ $18 + constant GPIOJ_BSRR ( write-only )  \ GPIO port bit set/reset register
    GPIOJ $1C + constant GPIOJ_LCKR ( read-write )  \ GPIO port configuration lock register
    GPIOJ $20 + constant GPIOJ_AFRL ( read-write )  \ GPIO alternate function lowregister
    GPIOJ $24 + constant GPIOJ_AFRH ( read-write )  \ GPIO alternate function high register
    GPIOJ $28 + constant GPIOJ_BRR ( read-write )  \ GPIO port bit reset register
    : GPIOJ_MODER. cr ." GPIOJ_MODER.  RW   $" GPIOJ_MODER @ hex. GPIOJ_MODER 1b. ;
    : GPIOJ_OTYPER. cr ." GPIOJ_OTYPER.  RW   $" GPIOJ_OTYPER @ hex. GPIOJ_OTYPER 1b. ;
    : GPIOJ_GPIOB_OSPEEDR. cr ." GPIOJ_GPIOB_OSPEEDR.  RW   $" GPIOJ_GPIOB_OSPEEDR @ hex. GPIOJ_GPIOB_OSPEEDR 1b. ;
    : GPIOJ_PUPDR. cr ." GPIOJ_PUPDR.  RW   $" GPIOJ_PUPDR @ hex. GPIOJ_PUPDR 1b. ;
    : GPIOJ_IDR. cr ." GPIOJ_IDR.  RO   $" GPIOJ_IDR @ hex. GPIOJ_IDR 1b. ;
    : GPIOJ_ODR. cr ." GPIOJ_ODR.  RW   $" GPIOJ_ODR @ hex. GPIOJ_ODR 1b. ;
    : GPIOJ_BSRR. cr ." GPIOJ_BSRR " WRITEONLY ; 
    : GPIOJ_LCKR. cr ." GPIOJ_LCKR.  RW   $" GPIOJ_LCKR @ hex. GPIOJ_LCKR 1b. ;
    : GPIOJ_AFRL. cr ." GPIOJ_AFRL.  RW   $" GPIOJ_AFRL @ hex. GPIOJ_AFRL 1b. ;
    : GPIOJ_AFRH. cr ." GPIOJ_AFRH.  RW   $" GPIOJ_AFRH @ hex. GPIOJ_AFRH 1b. ;
    : GPIOJ_BRR. cr ." GPIOJ_BRR.  RW   $" GPIOJ_BRR @ hex. GPIOJ_BRR 1b. ;
    : GPIOJ.
      GPIOJ_MODER.
      GPIOJ_OTYPER.
      GPIOJ_GPIOB_OSPEEDR.
      GPIOJ_PUPDR.
      GPIOJ_IDR.
      GPIOJ_ODR.
      GPIOJ_BSRR.
      GPIOJ_LCKR.
      GPIOJ_AFRL.
      GPIOJ_AFRH.
      GPIOJ_BRR.
    ;
  [then]

  execute-defined? use-GPIOI [if]
    $40022000 constant GPIOI ( General-purpose I/Os ) 
    GPIOI $0 + constant GPIOI_MODER ( read-write )  \ GPIO port mode register
    GPIOI $4 + constant GPIOI_OTYPER ( read-write )  \ GPIO port output type register
    GPIOI $8 + constant GPIOI_GPIOB_OSPEEDR ( read-write )  \ GPIO port output speed register
    GPIOI $C + constant GPIOI_PUPDR ( read-write )  \ GPIO port pull-up/pull-down register
    GPIOI $10 + constant GPIOI_IDR ( read-only )  \ GPIO port input data register
    GPIOI $14 + constant GPIOI_ODR ( read-write )  \ GPIO port output data register
    GPIOI $18 + constant GPIOI_BSRR ( write-only )  \ GPIO port bit set/reset register
    GPIOI $1C + constant GPIOI_LCKR ( read-write )  \ GPIO port configuration lock register
    GPIOI $20 + constant GPIOI_AFRL ( read-write )  \ GPIO alternate function lowregister
    GPIOI $24 + constant GPIOI_AFRH ( read-write )  \ GPIO alternate function high register
    GPIOI $28 + constant GPIOI_BRR ( read-write )  \ GPIO port bit reset register
    : GPIOI_MODER. cr ." GPIOI_MODER.  RW   $" GPIOI_MODER @ hex. GPIOI_MODER 1b. ;
    : GPIOI_OTYPER. cr ." GPIOI_OTYPER.  RW   $" GPIOI_OTYPER @ hex. GPIOI_OTYPER 1b. ;
    : GPIOI_GPIOB_OSPEEDR. cr ." GPIOI_GPIOB_OSPEEDR.  RW   $" GPIOI_GPIOB_OSPEEDR @ hex. GPIOI_GPIOB_OSPEEDR 1b. ;
    : GPIOI_PUPDR. cr ." GPIOI_PUPDR.  RW   $" GPIOI_PUPDR @ hex. GPIOI_PUPDR 1b. ;
    : GPIOI_IDR. cr ." GPIOI_IDR.  RO   $" GPIOI_IDR @ hex. GPIOI_IDR 1b. ;
    : GPIOI_ODR. cr ." GPIOI_ODR.  RW   $" GPIOI_ODR @ hex. GPIOI_ODR 1b. ;
    : GPIOI_BSRR. cr ." GPIOI_BSRR " WRITEONLY ; 
    : GPIOI_LCKR. cr ." GPIOI_LCKR.  RW   $" GPIOI_LCKR @ hex. GPIOI_LCKR 1b. ;
    : GPIOI_AFRL. cr ." GPIOI_AFRL.  RW   $" GPIOI_AFRL @ hex. GPIOI_AFRL 1b. ;
    : GPIOI_AFRH. cr ." GPIOI_AFRH.  RW   $" GPIOI_AFRH @ hex. GPIOI_AFRH 1b. ;
    : GPIOI_BRR. cr ." GPIOI_BRR.  RW   $" GPIOI_BRR @ hex. GPIOI_BRR 1b. ;
    : GPIOI.
      GPIOI_MODER.
      GPIOI_OTYPER.
      GPIOI_GPIOB_OSPEEDR.
      GPIOI_PUPDR.
      GPIOI_IDR.
      GPIOI_ODR.
      GPIOI_BSRR.
      GPIOI_LCKR.
      GPIOI_AFRL.
      GPIOI_AFRH.
      GPIOI_BRR.
    ;
  [then]

  execute-defined? use-GPIOH [if]
    $40021C00 constant GPIOH ( General-purpose I/Os ) 
    GPIOH $0 + constant GPIOH_MODER ( read-write )  \ GPIO port mode register
    GPIOH $4 + constant GPIOH_OTYPER ( read-write )  \ GPIO port output type register
    GPIOH $8 + constant GPIOH_GPIOB_OSPEEDR ( read-write )  \ GPIO port output speed register
    GPIOH $C + constant GPIOH_PUPDR ( read-write )  \ GPIO port pull-up/pull-down register
    GPIOH $10 + constant GPIOH_IDR ( read-only )  \ GPIO port input data register
    GPIOH $14 + constant GPIOH_ODR ( read-write )  \ GPIO port output data register
    GPIOH $18 + constant GPIOH_BSRR ( write-only )  \ GPIO port bit set/reset register
    GPIOH $1C + constant GPIOH_LCKR ( read-write )  \ GPIO port configuration lock register
    GPIOH $20 + constant GPIOH_AFRL ( read-write )  \ GPIO alternate function lowregister
    GPIOH $24 + constant GPIOH_AFRH ( read-write )  \ GPIO alternate function high register
    GPIOH $28 + constant GPIOH_BRR ( read-write )  \ GPIO port bit reset register
    : GPIOH_MODER. cr ." GPIOH_MODER.  RW   $" GPIOH_MODER @ hex. GPIOH_MODER 1b. ;
    : GPIOH_OTYPER. cr ." GPIOH_OTYPER.  RW   $" GPIOH_OTYPER @ hex. GPIOH_OTYPER 1b. ;
    : GPIOH_GPIOB_OSPEEDR. cr ." GPIOH_GPIOB_OSPEEDR.  RW   $" GPIOH_GPIOB_OSPEEDR @ hex. GPIOH_GPIOB_OSPEEDR 1b. ;
    : GPIOH_PUPDR. cr ." GPIOH_PUPDR.  RW   $" GPIOH_PUPDR @ hex. GPIOH_PUPDR 1b. ;
    : GPIOH_IDR. cr ." GPIOH_IDR.  RO   $" GPIOH_IDR @ hex. GPIOH_IDR 1b. ;
    : GPIOH_ODR. cr ." GPIOH_ODR.  RW   $" GPIOH_ODR @ hex. GPIOH_ODR 1b. ;
    : GPIOH_BSRR. cr ." GPIOH_BSRR " WRITEONLY ; 
    : GPIOH_LCKR. cr ." GPIOH_LCKR.  RW   $" GPIOH_LCKR @ hex. GPIOH_LCKR 1b. ;
    : GPIOH_AFRL. cr ." GPIOH_AFRL.  RW   $" GPIOH_AFRL @ hex. GPIOH_AFRL 1b. ;
    : GPIOH_AFRH. cr ." GPIOH_AFRH.  RW   $" GPIOH_AFRH @ hex. GPIOH_AFRH 1b. ;
    : GPIOH_BRR. cr ." GPIOH_BRR.  RW   $" GPIOH_BRR @ hex. GPIOH_BRR 1b. ;
    : GPIOH.
      GPIOH_MODER.
      GPIOH_OTYPER.
      GPIOH_GPIOB_OSPEEDR.
      GPIOH_PUPDR.
      GPIOH_IDR.
      GPIOH_ODR.
      GPIOH_BSRR.
      GPIOH_LCKR.
      GPIOH_AFRL.
      GPIOH_AFRH.
      GPIOH_BRR.
    ;
  [then]

  execute-defined? use-GPIOG [if]
    $40021800 constant GPIOG ( General-purpose I/Os ) 
    GPIOG $0 + constant GPIOG_MODER ( read-write )  \ GPIO port mode register
    GPIOG $4 + constant GPIOG_OTYPER ( read-write )  \ GPIO port output type register
    GPIOG $8 + constant GPIOG_GPIOB_OSPEEDR ( read-write )  \ GPIO port output speed register
    GPIOG $C + constant GPIOG_PUPDR ( read-write )  \ GPIO port pull-up/pull-down register
    GPIOG $10 + constant GPIOG_IDR ( read-only )  \ GPIO port input data register
    GPIOG $14 + constant GPIOG_ODR ( read-write )  \ GPIO port output data register
    GPIOG $18 + constant GPIOG_BSRR ( write-only )  \ GPIO port bit set/reset register
    GPIOG $1C + constant GPIOG_LCKR ( read-write )  \ GPIO port configuration lock register
    GPIOG $20 + constant GPIOG_AFRL ( read-write )  \ GPIO alternate function lowregister
    GPIOG $24 + constant GPIOG_AFRH ( read-write )  \ GPIO alternate function high register
    GPIOG $28 + constant GPIOG_BRR ( read-write )  \ GPIO port bit reset register
    : GPIOG_MODER. cr ." GPIOG_MODER.  RW   $" GPIOG_MODER @ hex. GPIOG_MODER 1b. ;
    : GPIOG_OTYPER. cr ." GPIOG_OTYPER.  RW   $" GPIOG_OTYPER @ hex. GPIOG_OTYPER 1b. ;
    : GPIOG_GPIOB_OSPEEDR. cr ." GPIOG_GPIOB_OSPEEDR.  RW   $" GPIOG_GPIOB_OSPEEDR @ hex. GPIOG_GPIOB_OSPEEDR 1b. ;
    : GPIOG_PUPDR. cr ." GPIOG_PUPDR.  RW   $" GPIOG_PUPDR @ hex. GPIOG_PUPDR 1b. ;
    : GPIOG_IDR. cr ." GPIOG_IDR.  RO   $" GPIOG_IDR @ hex. GPIOG_IDR 1b. ;
    : GPIOG_ODR. cr ." GPIOG_ODR.  RW   $" GPIOG_ODR @ hex. GPIOG_ODR 1b. ;
    : GPIOG_BSRR. cr ." GPIOG_BSRR " WRITEONLY ; 
    : GPIOG_LCKR. cr ." GPIOG_LCKR.  RW   $" GPIOG_LCKR @ hex. GPIOG_LCKR 1b. ;
    : GPIOG_AFRL. cr ." GPIOG_AFRL.  RW   $" GPIOG_AFRL @ hex. GPIOG_AFRL 1b. ;
    : GPIOG_AFRH. cr ." GPIOG_AFRH.  RW   $" GPIOG_AFRH @ hex. GPIOG_AFRH 1b. ;
    : GPIOG_BRR. cr ." GPIOG_BRR.  RW   $" GPIOG_BRR @ hex. GPIOG_BRR 1b. ;
    : GPIOG.
      GPIOG_MODER.
      GPIOG_OTYPER.
      GPIOG_GPIOB_OSPEEDR.
      GPIOG_PUPDR.
      GPIOG_IDR.
      GPIOG_ODR.
      GPIOG_BSRR.
      GPIOG_LCKR.
      GPIOG_AFRL.
      GPIOG_AFRH.
      GPIOG_BRR.
    ;
  [then]

  execute-defined? use-GPIOF [if]
    $40021400 constant GPIOF ( General-purpose I/Os ) 
    GPIOF $0 + constant GPIOF_MODER ( read-write )  \ GPIO port mode register
    GPIOF $4 + constant GPIOF_OTYPER ( read-write )  \ GPIO port output type register
    GPIOF $8 + constant GPIOF_GPIOB_OSPEEDR ( read-write )  \ GPIO port output speed register
    GPIOF $C + constant GPIOF_PUPDR ( read-write )  \ GPIO port pull-up/pull-down register
    GPIOF $10 + constant GPIOF_IDR ( read-only )  \ GPIO port input data register
    GPIOF $14 + constant GPIOF_ODR ( read-write )  \ GPIO port output data register
    GPIOF $18 + constant GPIOF_BSRR ( write-only )  \ GPIO port bit set/reset register
    GPIOF $1C + constant GPIOF_LCKR ( read-write )  \ GPIO port configuration lock register
    GPIOF $20 + constant GPIOF_AFRL ( read-write )  \ GPIO alternate function lowregister
    GPIOF $24 + constant GPIOF_AFRH ( read-write )  \ GPIO alternate function high register
    GPIOF $28 + constant GPIOF_BRR ( read-write )  \ GPIO port bit reset register
    : GPIOF_MODER. cr ." GPIOF_MODER.  RW   $" GPIOF_MODER @ hex. GPIOF_MODER 1b. ;
    : GPIOF_OTYPER. cr ." GPIOF_OTYPER.  RW   $" GPIOF_OTYPER @ hex. GPIOF_OTYPER 1b. ;
    : GPIOF_GPIOB_OSPEEDR. cr ." GPIOF_GPIOB_OSPEEDR.  RW   $" GPIOF_GPIOB_OSPEEDR @ hex. GPIOF_GPIOB_OSPEEDR 1b. ;
    : GPIOF_PUPDR. cr ." GPIOF_PUPDR.  RW   $" GPIOF_PUPDR @ hex. GPIOF_PUPDR 1b. ;
    : GPIOF_IDR. cr ." GPIOF_IDR.  RO   $" GPIOF_IDR @ hex. GPIOF_IDR 1b. ;
    : GPIOF_ODR. cr ." GPIOF_ODR.  RW   $" GPIOF_ODR @ hex. GPIOF_ODR 1b. ;
    : GPIOF_BSRR. cr ." GPIOF_BSRR " WRITEONLY ; 
    : GPIOF_LCKR. cr ." GPIOF_LCKR.  RW   $" GPIOF_LCKR @ hex. GPIOF_LCKR 1b. ;
    : GPIOF_AFRL. cr ." GPIOF_AFRL.  RW   $" GPIOF_AFRL @ hex. GPIOF_AFRL 1b. ;
    : GPIOF_AFRH. cr ." GPIOF_AFRH.  RW   $" GPIOF_AFRH @ hex. GPIOF_AFRH 1b. ;
    : GPIOF_BRR. cr ." GPIOF_BRR.  RW   $" GPIOF_BRR @ hex. GPIOF_BRR 1b. ;
    : GPIOF.
      GPIOF_MODER.
      GPIOF_OTYPER.
      GPIOF_GPIOB_OSPEEDR.
      GPIOF_PUPDR.
      GPIOF_IDR.
      GPIOF_ODR.
      GPIOF_BSRR.
      GPIOF_LCKR.
      GPIOF_AFRL.
      GPIOF_AFRH.
      GPIOF_BRR.
    ;
  [then]

  execute-defined? use-GPIOE [if]
    $40021000 constant GPIOE ( General-purpose I/Os ) 
    GPIOE $0 + constant GPIOE_MODER ( read-write )  \ GPIO port mode register
    GPIOE $4 + constant GPIOE_OTYPER ( read-write )  \ GPIO port output type register
    GPIOE $8 + constant GPIOE_GPIOB_OSPEEDR ( read-write )  \ GPIO port output speed register
    GPIOE $C + constant GPIOE_PUPDR ( read-write )  \ GPIO port pull-up/pull-down register
    GPIOE $10 + constant GPIOE_IDR ( read-only )  \ GPIO port input data register
    GPIOE $14 + constant GPIOE_ODR ( read-write )  \ GPIO port output data register
    GPIOE $18 + constant GPIOE_BSRR ( write-only )  \ GPIO port bit set/reset register
    GPIOE $1C + constant GPIOE_LCKR ( read-write )  \ GPIO port configuration lock register
    GPIOE $20 + constant GPIOE_AFRL ( read-write )  \ GPIO alternate function lowregister
    GPIOE $24 + constant GPIOE_AFRH ( read-write )  \ GPIO alternate function high register
    GPIOE $28 + constant GPIOE_BRR ( read-write )  \ GPIO port bit reset register
    : GPIOE_MODER. cr ." GPIOE_MODER.  RW   $" GPIOE_MODER @ hex. GPIOE_MODER 1b. ;
    : GPIOE_OTYPER. cr ." GPIOE_OTYPER.  RW   $" GPIOE_OTYPER @ hex. GPIOE_OTYPER 1b. ;
    : GPIOE_GPIOB_OSPEEDR. cr ." GPIOE_GPIOB_OSPEEDR.  RW   $" GPIOE_GPIOB_OSPEEDR @ hex. GPIOE_GPIOB_OSPEEDR 1b. ;
    : GPIOE_PUPDR. cr ." GPIOE_PUPDR.  RW   $" GPIOE_PUPDR @ hex. GPIOE_PUPDR 1b. ;
    : GPIOE_IDR. cr ." GPIOE_IDR.  RO   $" GPIOE_IDR @ hex. GPIOE_IDR 1b. ;
    : GPIOE_ODR. cr ." GPIOE_ODR.  RW   $" GPIOE_ODR @ hex. GPIOE_ODR 1b. ;
    : GPIOE_BSRR. cr ." GPIOE_BSRR " WRITEONLY ; 
    : GPIOE_LCKR. cr ." GPIOE_LCKR.  RW   $" GPIOE_LCKR @ hex. GPIOE_LCKR 1b. ;
    : GPIOE_AFRL. cr ." GPIOE_AFRL.  RW   $" GPIOE_AFRL @ hex. GPIOE_AFRL 1b. ;
    : GPIOE_AFRH. cr ." GPIOE_AFRH.  RW   $" GPIOE_AFRH @ hex. GPIOE_AFRH 1b. ;
    : GPIOE_BRR. cr ." GPIOE_BRR.  RW   $" GPIOE_BRR @ hex. GPIOE_BRR 1b. ;
    : GPIOE.
      GPIOE_MODER.
      GPIOE_OTYPER.
      GPIOE_GPIOB_OSPEEDR.
      GPIOE_PUPDR.
      GPIOE_IDR.
      GPIOE_ODR.
      GPIOE_BSRR.
      GPIOE_LCKR.
      GPIOE_AFRL.
      GPIOE_AFRH.
      GPIOE_BRR.
    ;
  [then]

  execute-defined? use-GPIOB [if]
    $40020400 constant GPIOB ( General-purpose I/Os ) 
    GPIOB $0 + constant GPIOB_MODER ( read-write )  \ GPIO port mode register
    GPIOB $4 + constant GPIOB_OTYPER ( read-write )  \ GPIO port output type register
    GPIOB $8 + constant GPIOB_GPIOB_OSPEEDR ( read-write )  \ GPIO port output speed register
    GPIOB $C + constant GPIOB_PUPDR ( read-write )  \ GPIO port pull-up/pull-down register
    GPIOB $10 + constant GPIOB_IDR ( read-only )  \ GPIO port input data register
    GPIOB $14 + constant GPIOB_ODR ( read-write )  \ GPIO port output data register
    GPIOB $18 + constant GPIOB_BSRR ( write-only )  \ GPIO port bit set/reset register
    GPIOB $1C + constant GPIOB_LCKR ( read-write )  \ GPIO port configuration lock register
    GPIOB $20 + constant GPIOB_AFRL ( read-write )  \ GPIO alternate function low register
    GPIOB $24 + constant GPIOB_AFRH ( read-write )  \ GPIO alternate function high register
    GPIOB $28 + constant GPIOB_BRR ( read-write )  \ GPIO port bit reset register
    : GPIOB_MODER. cr ." GPIOB_MODER.  RW   $" GPIOB_MODER @ hex. GPIOB_MODER 1b. ;
    : GPIOB_OTYPER. cr ." GPIOB_OTYPER.  RW   $" GPIOB_OTYPER @ hex. GPIOB_OTYPER 1b. ;
    : GPIOB_GPIOB_OSPEEDR. cr ." GPIOB_GPIOB_OSPEEDR.  RW   $" GPIOB_GPIOB_OSPEEDR @ hex. GPIOB_GPIOB_OSPEEDR 1b. ;
    : GPIOB_PUPDR. cr ." GPIOB_PUPDR.  RW   $" GPIOB_PUPDR @ hex. GPIOB_PUPDR 1b. ;
    : GPIOB_IDR. cr ." GPIOB_IDR.  RO   $" GPIOB_IDR @ hex. GPIOB_IDR 1b. ;
    : GPIOB_ODR. cr ." GPIOB_ODR.  RW   $" GPIOB_ODR @ hex. GPIOB_ODR 1b. ;
    : GPIOB_BSRR. cr ." GPIOB_BSRR " WRITEONLY ; 
    : GPIOB_LCKR. cr ." GPIOB_LCKR.  RW   $" GPIOB_LCKR @ hex. GPIOB_LCKR 1b. ;
    : GPIOB_AFRL. cr ." GPIOB_AFRL.  RW   $" GPIOB_AFRL @ hex. GPIOB_AFRL 1b. ;
    : GPIOB_AFRH. cr ." GPIOB_AFRH.  RW   $" GPIOB_AFRH @ hex. GPIOB_AFRH 1b. ;
    : GPIOB_BRR. cr ." GPIOB_BRR.  RW   $" GPIOB_BRR @ hex. GPIOB_BRR 1b. ;
    : GPIOB.
      GPIOB_MODER.
      GPIOB_OTYPER.
      GPIOB_GPIOB_OSPEEDR.
      GPIOB_PUPDR.
      GPIOB_IDR.
      GPIOB_ODR.
      GPIOB_BSRR.
      GPIOB_LCKR.
      GPIOB_AFRL.
      GPIOB_AFRH.
      GPIOB_BRR.
    ;
  [then]

  execute-defined? use-GPIOA [if]
    $40020000 constant GPIOA ( General-purpose I/Os ) 
    GPIOA $0 + constant GPIOA_MODER ( read-write )  \ GPIO port mode register
    GPIOA $4 + constant GPIOA_OTYPER ( read-write )  \ GPIO port output type register
    GPIOA $8 + constant GPIOA_GPIOB_OSPEEDR ( read-write )  \ GPIO port output speed register
    GPIOA $C + constant GPIOA_PUPDR ( read-write )  \ GPIO port pull-up/pull-down register
    GPIOA $10 + constant GPIOA_IDR ( read-only )  \ GPIO port input data register
    GPIOA $14 + constant GPIOA_ODR ( read-write )  \ GPIO port output data register
    GPIOA $18 + constant GPIOA_BSRR ( write-only )  \ GPIO port bit set/reset register
    GPIOA $1C + constant GPIOA_LCKR ( read-write )  \ GPIO port configuration lock register
    GPIOA $20 + constant GPIOA_AFRL ( read-write )  \ GPIO alternate function low register
    GPIOA $24 + constant GPIOA_AFRH ( read-write )  \ GPIO alternate function high register
    GPIOA $28 + constant GPIOA_BRR ( read-write )  \ GPIO port bit reset register
    : GPIOA_MODER. cr ." GPIOA_MODER.  RW   $" GPIOA_MODER @ hex. GPIOA_MODER 1b. ;
    : GPIOA_OTYPER. cr ." GPIOA_OTYPER.  RW   $" GPIOA_OTYPER @ hex. GPIOA_OTYPER 1b. ;
    : GPIOA_GPIOB_OSPEEDR. cr ." GPIOA_GPIOB_OSPEEDR.  RW   $" GPIOA_GPIOB_OSPEEDR @ hex. GPIOA_GPIOB_OSPEEDR 1b. ;
    : GPIOA_PUPDR. cr ." GPIOA_PUPDR.  RW   $" GPIOA_PUPDR @ hex. GPIOA_PUPDR 1b. ;
    : GPIOA_IDR. cr ." GPIOA_IDR.  RO   $" GPIOA_IDR @ hex. GPIOA_IDR 1b. ;
    : GPIOA_ODR. cr ." GPIOA_ODR.  RW   $" GPIOA_ODR @ hex. GPIOA_ODR 1b. ;
    : GPIOA_BSRR. cr ." GPIOA_BSRR " WRITEONLY ; 
    : GPIOA_LCKR. cr ." GPIOA_LCKR.  RW   $" GPIOA_LCKR @ hex. GPIOA_LCKR 1b. ;
    : GPIOA_AFRL. cr ." GPIOA_AFRL.  RW   $" GPIOA_AFRL @ hex. GPIOA_AFRL 1b. ;
    : GPIOA_AFRH. cr ." GPIOA_AFRH.  RW   $" GPIOA_AFRH @ hex. GPIOA_AFRH 1b. ;
    : GPIOA_BRR. cr ." GPIOA_BRR.  RW   $" GPIOA_BRR @ hex. GPIOA_BRR 1b. ;
    : GPIOA.
      GPIOA_MODER.
      GPIOA_OTYPER.
      GPIOA_GPIOB_OSPEEDR.
      GPIOA_PUPDR.
      GPIOA_IDR.
      GPIOA_ODR.
      GPIOA_BSRR.
      GPIOA_LCKR.
      GPIOA_AFRL.
      GPIOA_AFRH.
      GPIOA_BRR.
    ;
  [then]

  execute-defined? use-SYSCFG [if]
    $40013800 constant SYSCFG ( System configuration controller ) 
    SYSCFG $0 + constant SYSCFG_MEMRM ( read-write )  \ memory remap register
    SYSCFG $4 + constant SYSCFG_PMC ( read-write )  \ peripheral mode configuration register
    SYSCFG $8 + constant SYSCFG_EXTICR1 ( read-write )  \ external interrupt configuration register 1
    SYSCFG $C + constant SYSCFG_EXTICR2 ( read-write )  \ external interrupt configuration register 2
    SYSCFG $10 + constant SYSCFG_EXTICR3 ( read-write )  \ external interrupt configuration register 3
    SYSCFG $14 + constant SYSCFG_EXTICR4 ( read-write )  \ external interrupt configuration register 4
    SYSCFG $20 + constant SYSCFG_CMPCR ( read-only )  \ Compensation cell control register
    : SYSCFG_MEMRM. cr ." SYSCFG_MEMRM.  RW   $" SYSCFG_MEMRM @ hex. SYSCFG_MEMRM 1b. ;
    : SYSCFG_PMC. cr ." SYSCFG_PMC.  RW   $" SYSCFG_PMC @ hex. SYSCFG_PMC 1b. ;
    : SYSCFG_EXTICR1. cr ." SYSCFG_EXTICR1.  RW   $" SYSCFG_EXTICR1 @ hex. SYSCFG_EXTICR1 1b. ;
    : SYSCFG_EXTICR2. cr ." SYSCFG_EXTICR2.  RW   $" SYSCFG_EXTICR2 @ hex. SYSCFG_EXTICR2 1b. ;
    : SYSCFG_EXTICR3. cr ." SYSCFG_EXTICR3.  RW   $" SYSCFG_EXTICR3 @ hex. SYSCFG_EXTICR3 1b. ;
    : SYSCFG_EXTICR4. cr ." SYSCFG_EXTICR4.  RW   $" SYSCFG_EXTICR4 @ hex. SYSCFG_EXTICR4 1b. ;
    : SYSCFG_CMPCR. cr ." SYSCFG_CMPCR.  RO   $" SYSCFG_CMPCR @ hex. SYSCFG_CMPCR 1b. ;
    : SYSCFG.
      SYSCFG_MEMRM.
      SYSCFG_PMC.
      SYSCFG_EXTICR1.
      SYSCFG_EXTICR2.
      SYSCFG_EXTICR3.
      SYSCFG_EXTICR4.
      SYSCFG_CMPCR.
    ;
  [then]

  execute-defined? use-SPI1 [if]
    $40013000 constant SPI1 ( Serial peripheral interface ) 
    SPI1 $0 + constant SPI1_CR1 ( read-write )  \ control register 1
    SPI1 $4 + constant SPI1_CR2 ( read-write )  \ control register 2
    SPI1 $8 + constant SPI1_SR (  )  \ status register
    SPI1 $C + constant SPI1_DR ( read-write )  \ data register
    SPI1 $10 + constant SPI1_CRCPR ( read-write )  \ CRC polynomial register
    SPI1 $14 + constant SPI1_RXCRCR ( read-only )  \ RX CRC register
    SPI1 $18 + constant SPI1_TXCRCR ( read-only )  \ TX CRC register
    SPI1 $1C + constant SPI1_I2SCFGR ( read-write )  \ I2S configuration register
    SPI1 $20 + constant SPI1_I2SPR ( read-write )  \ I2S prescaler register
    : SPI1_CR1. cr ." SPI1_CR1.  RW   $" SPI1_CR1 @ hex. SPI1_CR1 1b. ;
    : SPI1_CR2. cr ." SPI1_CR2.  RW   $" SPI1_CR2 @ hex. SPI1_CR2 1b. ;
    : SPI1_SR. cr ." SPI1_SR.   $" SPI1_SR @ hex. SPI1_SR 1b. ;
    : SPI1_DR. cr ." SPI1_DR.  RW   $" SPI1_DR @ hex. SPI1_DR 1b. ;
    : SPI1_CRCPR. cr ." SPI1_CRCPR.  RW   $" SPI1_CRCPR @ hex. SPI1_CRCPR 1b. ;
    : SPI1_RXCRCR. cr ." SPI1_RXCRCR.  RO   $" SPI1_RXCRCR @ hex. SPI1_RXCRCR 1b. ;
    : SPI1_TXCRCR. cr ." SPI1_TXCRCR.  RO   $" SPI1_TXCRCR @ hex. SPI1_TXCRCR 1b. ;
    : SPI1_I2SCFGR. cr ." SPI1_I2SCFGR.  RW   $" SPI1_I2SCFGR @ hex. SPI1_I2SCFGR 1b. ;
    : SPI1_I2SPR. cr ." SPI1_I2SPR.  RW   $" SPI1_I2SPR @ hex. SPI1_I2SPR 1b. ;
    : SPI1.
      SPI1_CR1.
      SPI1_CR2.
      SPI1_SR.
      SPI1_DR.
      SPI1_CRCPR.
      SPI1_RXCRCR.
      SPI1_TXCRCR.
      SPI1_I2SCFGR.
      SPI1_I2SPR.
    ;
  [then]

  execute-defined? use-SPI3 [if]
    $40003C00 constant SPI3 ( Serial peripheral interface ) 
    SPI3 $0 + constant SPI3_CR1 ( read-write )  \ control register 1
    SPI3 $4 + constant SPI3_CR2 ( read-write )  \ control register 2
    SPI3 $8 + constant SPI3_SR (  )  \ status register
    SPI3 $C + constant SPI3_DR ( read-write )  \ data register
    SPI3 $10 + constant SPI3_CRCPR ( read-write )  \ CRC polynomial register
    SPI3 $14 + constant SPI3_RXCRCR ( read-only )  \ RX CRC register
    SPI3 $18 + constant SPI3_TXCRCR ( read-only )  \ TX CRC register
    SPI3 $1C + constant SPI3_I2SCFGR ( read-write )  \ I2S configuration register
    SPI3 $20 + constant SPI3_I2SPR ( read-write )  \ I2S prescaler register
    : SPI3_CR1. cr ." SPI3_CR1.  RW   $" SPI3_CR1 @ hex. SPI3_CR1 1b. ;
    : SPI3_CR2. cr ." SPI3_CR2.  RW   $" SPI3_CR2 @ hex. SPI3_CR2 1b. ;
    : SPI3_SR. cr ." SPI3_SR.   $" SPI3_SR @ hex. SPI3_SR 1b. ;
    : SPI3_DR. cr ." SPI3_DR.  RW   $" SPI3_DR @ hex. SPI3_DR 1b. ;
    : SPI3_CRCPR. cr ." SPI3_CRCPR.  RW   $" SPI3_CRCPR @ hex. SPI3_CRCPR 1b. ;
    : SPI3_RXCRCR. cr ." SPI3_RXCRCR.  RO   $" SPI3_RXCRCR @ hex. SPI3_RXCRCR 1b. ;
    : SPI3_TXCRCR. cr ." SPI3_TXCRCR.  RO   $" SPI3_TXCRCR @ hex. SPI3_TXCRCR 1b. ;
    : SPI3_I2SCFGR. cr ." SPI3_I2SCFGR.  RW   $" SPI3_I2SCFGR @ hex. SPI3_I2SCFGR 1b. ;
    : SPI3_I2SPR. cr ." SPI3_I2SPR.  RW   $" SPI3_I2SPR @ hex. SPI3_I2SPR 1b. ;
    : SPI3.
      SPI3_CR1.
      SPI3_CR2.
      SPI3_SR.
      SPI3_DR.
      SPI3_CRCPR.
      SPI3_RXCRCR.
      SPI3_TXCRCR.
      SPI3_I2SCFGR.
      SPI3_I2SPR.
    ;
  [then]

  execute-defined? use-SPI4 [if]
    $40013400 constant SPI4 ( Serial peripheral interface ) 
    SPI4 $0 + constant SPI4_CR1 ( read-write )  \ control register 1
    SPI4 $4 + constant SPI4_CR2 ( read-write )  \ control register 2
    SPI4 $8 + constant SPI4_SR (  )  \ status register
    SPI4 $C + constant SPI4_DR ( read-write )  \ data register
    SPI4 $10 + constant SPI4_CRCPR ( read-write )  \ CRC polynomial register
    SPI4 $14 + constant SPI4_RXCRCR ( read-only )  \ RX CRC register
    SPI4 $18 + constant SPI4_TXCRCR ( read-only )  \ TX CRC register
    SPI4 $1C + constant SPI4_I2SCFGR ( read-write )  \ I2S configuration register
    SPI4 $20 + constant SPI4_I2SPR ( read-write )  \ I2S prescaler register
    : SPI4_CR1. cr ." SPI4_CR1.  RW   $" SPI4_CR1 @ hex. SPI4_CR1 1b. ;
    : SPI4_CR2. cr ." SPI4_CR2.  RW   $" SPI4_CR2 @ hex. SPI4_CR2 1b. ;
    : SPI4_SR. cr ." SPI4_SR.   $" SPI4_SR @ hex. SPI4_SR 1b. ;
    : SPI4_DR. cr ." SPI4_DR.  RW   $" SPI4_DR @ hex. SPI4_DR 1b. ;
    : SPI4_CRCPR. cr ." SPI4_CRCPR.  RW   $" SPI4_CRCPR @ hex. SPI4_CRCPR 1b. ;
    : SPI4_RXCRCR. cr ." SPI4_RXCRCR.  RO   $" SPI4_RXCRCR @ hex. SPI4_RXCRCR 1b. ;
    : SPI4_TXCRCR. cr ." SPI4_TXCRCR.  RO   $" SPI4_TXCRCR @ hex. SPI4_TXCRCR 1b. ;
    : SPI4_I2SCFGR. cr ." SPI4_I2SCFGR.  RW   $" SPI4_I2SCFGR @ hex. SPI4_I2SCFGR 1b. ;
    : SPI4_I2SPR. cr ." SPI4_I2SPR.  RW   $" SPI4_I2SPR @ hex. SPI4_I2SPR 1b. ;
    : SPI4.
      SPI4_CR1.
      SPI4_CR2.
      SPI4_SR.
      SPI4_DR.
      SPI4_CRCPR.
      SPI4_RXCRCR.
      SPI4_TXCRCR.
      SPI4_I2SCFGR.
      SPI4_I2SPR.
    ;
  [then]

  execute-defined? use-SPI5 [if]
    $40015000 constant SPI5 ( Serial peripheral interface ) 
    SPI5 $0 + constant SPI5_CR1 ( read-write )  \ control register 1
    SPI5 $4 + constant SPI5_CR2 ( read-write )  \ control register 2
    SPI5 $8 + constant SPI5_SR (  )  \ status register
    SPI5 $C + constant SPI5_DR ( read-write )  \ data register
    SPI5 $10 + constant SPI5_CRCPR ( read-write )  \ CRC polynomial register
    SPI5 $14 + constant SPI5_RXCRCR ( read-only )  \ RX CRC register
    SPI5 $18 + constant SPI5_TXCRCR ( read-only )  \ TX CRC register
    SPI5 $1C + constant SPI5_I2SCFGR ( read-write )  \ I2S configuration register
    SPI5 $20 + constant SPI5_I2SPR ( read-write )  \ I2S prescaler register
    : SPI5_CR1. cr ." SPI5_CR1.  RW   $" SPI5_CR1 @ hex. SPI5_CR1 1b. ;
    : SPI5_CR2. cr ." SPI5_CR2.  RW   $" SPI5_CR2 @ hex. SPI5_CR2 1b. ;
    : SPI5_SR. cr ." SPI5_SR.   $" SPI5_SR @ hex. SPI5_SR 1b. ;
    : SPI5_DR. cr ." SPI5_DR.  RW   $" SPI5_DR @ hex. SPI5_DR 1b. ;
    : SPI5_CRCPR. cr ." SPI5_CRCPR.  RW   $" SPI5_CRCPR @ hex. SPI5_CRCPR 1b. ;
    : SPI5_RXCRCR. cr ." SPI5_RXCRCR.  RO   $" SPI5_RXCRCR @ hex. SPI5_RXCRCR 1b. ;
    : SPI5_TXCRCR. cr ." SPI5_TXCRCR.  RO   $" SPI5_TXCRCR @ hex. SPI5_TXCRCR 1b. ;
    : SPI5_I2SCFGR. cr ." SPI5_I2SCFGR.  RW   $" SPI5_I2SCFGR @ hex. SPI5_I2SCFGR 1b. ;
    : SPI5_I2SPR. cr ." SPI5_I2SPR.  RW   $" SPI5_I2SPR @ hex. SPI5_I2SPR 1b. ;
    : SPI5.
      SPI5_CR1.
      SPI5_CR2.
      SPI5_SR.
      SPI5_DR.
      SPI5_CRCPR.
      SPI5_RXCRCR.
      SPI5_TXCRCR.
      SPI5_I2SCFGR.
      SPI5_I2SPR.
    ;
  [then]

  execute-defined? use-SPI6 [if]
    $40015400 constant SPI6 ( Serial peripheral interface ) 
    SPI6 $0 + constant SPI6_CR1 ( read-write )  \ control register 1
    SPI6 $4 + constant SPI6_CR2 ( read-write )  \ control register 2
    SPI6 $8 + constant SPI6_SR (  )  \ status register
    SPI6 $C + constant SPI6_DR ( read-write )  \ data register
    SPI6 $10 + constant SPI6_CRCPR ( read-write )  \ CRC polynomial register
    SPI6 $14 + constant SPI6_RXCRCR ( read-only )  \ RX CRC register
    SPI6 $18 + constant SPI6_TXCRCR ( read-only )  \ TX CRC register
    SPI6 $1C + constant SPI6_I2SCFGR ( read-write )  \ I2S configuration register
    SPI6 $20 + constant SPI6_I2SPR ( read-write )  \ I2S prescaler register
    : SPI6_CR1. cr ." SPI6_CR1.  RW   $" SPI6_CR1 @ hex. SPI6_CR1 1b. ;
    : SPI6_CR2. cr ." SPI6_CR2.  RW   $" SPI6_CR2 @ hex. SPI6_CR2 1b. ;
    : SPI6_SR. cr ." SPI6_SR.   $" SPI6_SR @ hex. SPI6_SR 1b. ;
    : SPI6_DR. cr ." SPI6_DR.  RW   $" SPI6_DR @ hex. SPI6_DR 1b. ;
    : SPI6_CRCPR. cr ." SPI6_CRCPR.  RW   $" SPI6_CRCPR @ hex. SPI6_CRCPR 1b. ;
    : SPI6_RXCRCR. cr ." SPI6_RXCRCR.  RO   $" SPI6_RXCRCR @ hex. SPI6_RXCRCR 1b. ;
    : SPI6_TXCRCR. cr ." SPI6_TXCRCR.  RO   $" SPI6_TXCRCR @ hex. SPI6_TXCRCR 1b. ;
    : SPI6_I2SCFGR. cr ." SPI6_I2SCFGR.  RW   $" SPI6_I2SCFGR @ hex. SPI6_I2SCFGR 1b. ;
    : SPI6_I2SPR. cr ." SPI6_I2SPR.  RW   $" SPI6_I2SPR @ hex. SPI6_I2SPR 1b. ;
    : SPI6.
      SPI6_CR1.
      SPI6_CR2.
      SPI6_SR.
      SPI6_DR.
      SPI6_CRCPR.
      SPI6_RXCRCR.
      SPI6_TXCRCR.
      SPI6_I2SCFGR.
      SPI6_I2SPR.
    ;
  [then]

  execute-defined? use-SPI2 [if]
    $40003800 constant SPI2 ( Serial peripheral interface ) 
    SPI2 $0 + constant SPI2_CR1 ( read-write )  \ control register 1
    SPI2 $4 + constant SPI2_CR2 ( read-write )  \ control register 2
    SPI2 $8 + constant SPI2_SR (  )  \ status register
    SPI2 $C + constant SPI2_DR ( read-write )  \ data register
    SPI2 $10 + constant SPI2_CRCPR ( read-write )  \ CRC polynomial register
    SPI2 $14 + constant SPI2_RXCRCR ( read-only )  \ RX CRC register
    SPI2 $18 + constant SPI2_TXCRCR ( read-only )  \ TX CRC register
    SPI2 $1C + constant SPI2_I2SCFGR ( read-write )  \ I2S configuration register
    SPI2 $20 + constant SPI2_I2SPR ( read-write )  \ I2S prescaler register
    : SPI2_CR1. cr ." SPI2_CR1.  RW   $" SPI2_CR1 @ hex. SPI2_CR1 1b. ;
    : SPI2_CR2. cr ." SPI2_CR2.  RW   $" SPI2_CR2 @ hex. SPI2_CR2 1b. ;
    : SPI2_SR. cr ." SPI2_SR.   $" SPI2_SR @ hex. SPI2_SR 1b. ;
    : SPI2_DR. cr ." SPI2_DR.  RW   $" SPI2_DR @ hex. SPI2_DR 1b. ;
    : SPI2_CRCPR. cr ." SPI2_CRCPR.  RW   $" SPI2_CRCPR @ hex. SPI2_CRCPR 1b. ;
    : SPI2_RXCRCR. cr ." SPI2_RXCRCR.  RO   $" SPI2_RXCRCR @ hex. SPI2_RXCRCR 1b. ;
    : SPI2_TXCRCR. cr ." SPI2_TXCRCR.  RO   $" SPI2_TXCRCR @ hex. SPI2_TXCRCR 1b. ;
    : SPI2_I2SCFGR. cr ." SPI2_I2SCFGR.  RW   $" SPI2_I2SCFGR @ hex. SPI2_I2SCFGR 1b. ;
    : SPI2_I2SPR. cr ." SPI2_I2SPR.  RW   $" SPI2_I2SPR @ hex. SPI2_I2SPR 1b. ;
    : SPI2.
      SPI2_CR1.
      SPI2_CR2.
      SPI2_SR.
      SPI2_DR.
      SPI2_CRCPR.
      SPI2_RXCRCR.
      SPI2_TXCRCR.
      SPI2_I2SCFGR.
      SPI2_I2SPR.
    ;
  [then]

  execute-defined? use-ADC1 [if]
    $40012000 constant ADC1 ( Analog-to-digital converter ) 
    ADC1 $0 + constant ADC1_SR ( read-write )  \ status register
    ADC1 $4 + constant ADC1_CR1 ( read-write )  \ control register 1
    ADC1 $8 + constant ADC1_CR2 ( read-write )  \ control register 2
    ADC1 $C + constant ADC1_SMPR1 ( read-write )  \ sample time register 1
    ADC1 $10 + constant ADC1_SMPR2 ( read-write )  \ sample time register 2
    ADC1 $14 + constant ADC1_JOFR1 ( read-write )  \ injected channel data offset register x
    ADC1 $18 + constant ADC1_JOFR2 ( read-write )  \ injected channel data offset register x
    ADC1 $1C + constant ADC1_JOFR3 ( read-write )  \ injected channel data offset register x
    ADC1 $20 + constant ADC1_JOFR4 ( read-write )  \ injected channel data offset register x
    ADC1 $24 + constant ADC1_HTR ( read-write )  \ watchdog higher threshold register
    ADC1 $28 + constant ADC1_LTR ( read-write )  \ watchdog lower threshold register
    ADC1 $2C + constant ADC1_SQR1 ( read-write )  \ regular sequence register 1
    ADC1 $30 + constant ADC1_SQR2 ( read-write )  \ regular sequence register 2
    ADC1 $34 + constant ADC1_SQR3 ( read-write )  \ regular sequence register 3
    ADC1 $38 + constant ADC1_JSQR ( read-write )  \ injected sequence register
    ADC1 $3C + constant ADC1_JDR1 ( read-only )  \ injected data register x
    ADC1 $40 + constant ADC1_JDR2 ( read-only )  \ injected data register x
    ADC1 $44 + constant ADC1_JDR3 ( read-only )  \ injected data register x
    ADC1 $48 + constant ADC1_JDR4 ( read-only )  \ injected data register x
    ADC1 $4C + constant ADC1_DR ( read-only )  \ regular data register
    : ADC1_SR. cr ." ADC1_SR.  RW   $" ADC1_SR @ hex. ADC1_SR 1b. ;
    : ADC1_CR1. cr ." ADC1_CR1.  RW   $" ADC1_CR1 @ hex. ADC1_CR1 1b. ;
    : ADC1_CR2. cr ." ADC1_CR2.  RW   $" ADC1_CR2 @ hex. ADC1_CR2 1b. ;
    : ADC1_SMPR1. cr ." ADC1_SMPR1.  RW   $" ADC1_SMPR1 @ hex. ADC1_SMPR1 1b. ;
    : ADC1_SMPR2. cr ." ADC1_SMPR2.  RW   $" ADC1_SMPR2 @ hex. ADC1_SMPR2 1b. ;
    : ADC1_JOFR1. cr ." ADC1_JOFR1.  RW   $" ADC1_JOFR1 @ hex. ADC1_JOFR1 1b. ;
    : ADC1_JOFR2. cr ." ADC1_JOFR2.  RW   $" ADC1_JOFR2 @ hex. ADC1_JOFR2 1b. ;
    : ADC1_JOFR3. cr ." ADC1_JOFR3.  RW   $" ADC1_JOFR3 @ hex. ADC1_JOFR3 1b. ;
    : ADC1_JOFR4. cr ." ADC1_JOFR4.  RW   $" ADC1_JOFR4 @ hex. ADC1_JOFR4 1b. ;
    : ADC1_HTR. cr ." ADC1_HTR.  RW   $" ADC1_HTR @ hex. ADC1_HTR 1b. ;
    : ADC1_LTR. cr ." ADC1_LTR.  RW   $" ADC1_LTR @ hex. ADC1_LTR 1b. ;
    : ADC1_SQR1. cr ." ADC1_SQR1.  RW   $" ADC1_SQR1 @ hex. ADC1_SQR1 1b. ;
    : ADC1_SQR2. cr ." ADC1_SQR2.  RW   $" ADC1_SQR2 @ hex. ADC1_SQR2 1b. ;
    : ADC1_SQR3. cr ." ADC1_SQR3.  RW   $" ADC1_SQR3 @ hex. ADC1_SQR3 1b. ;
    : ADC1_JSQR. cr ." ADC1_JSQR.  RW   $" ADC1_JSQR @ hex. ADC1_JSQR 1b. ;
    : ADC1_JDR1. cr ." ADC1_JDR1.  RO   $" ADC1_JDR1 @ hex. ADC1_JDR1 1b. ;
    : ADC1_JDR2. cr ." ADC1_JDR2.  RO   $" ADC1_JDR2 @ hex. ADC1_JDR2 1b. ;
    : ADC1_JDR3. cr ." ADC1_JDR3.  RO   $" ADC1_JDR3 @ hex. ADC1_JDR3 1b. ;
    : ADC1_JDR4. cr ." ADC1_JDR4.  RO   $" ADC1_JDR4 @ hex. ADC1_JDR4 1b. ;
    : ADC1_DR. cr ." ADC1_DR.  RO   $" ADC1_DR @ hex. ADC1_DR 1b. ;
    : ADC1.
      ADC1_SR.
      ADC1_CR1.
      ADC1_CR2.
      ADC1_SMPR1.
      ADC1_SMPR2.
      ADC1_JOFR1.
      ADC1_JOFR2.
      ADC1_JOFR3.
      ADC1_JOFR4.
      ADC1_HTR.
      ADC1_LTR.
      ADC1_SQR1.
      ADC1_SQR2.
      ADC1_SQR3.
      ADC1_JSQR.
      ADC1_JDR1.
      ADC1_JDR2.
      ADC1_JDR3.
      ADC1_JDR4.
      ADC1_DR.
    ;
  [then]

  execute-defined? use-ADC2 [if]
    $40012100 constant ADC2 ( Analog-to-digital converter ) 
    ADC2 $0 + constant ADC2_SR ( read-write )  \ status register
    ADC2 $4 + constant ADC2_CR1 ( read-write )  \ control register 1
    ADC2 $8 + constant ADC2_CR2 ( read-write )  \ control register 2
    ADC2 $C + constant ADC2_SMPR1 ( read-write )  \ sample time register 1
    ADC2 $10 + constant ADC2_SMPR2 ( read-write )  \ sample time register 2
    ADC2 $14 + constant ADC2_JOFR1 ( read-write )  \ injected channel data offset register x
    ADC2 $18 + constant ADC2_JOFR2 ( read-write )  \ injected channel data offset register x
    ADC2 $1C + constant ADC2_JOFR3 ( read-write )  \ injected channel data offset register x
    ADC2 $20 + constant ADC2_JOFR4 ( read-write )  \ injected channel data offset register x
    ADC2 $24 + constant ADC2_HTR ( read-write )  \ watchdog higher threshold register
    ADC2 $28 + constant ADC2_LTR ( read-write )  \ watchdog lower threshold register
    ADC2 $2C + constant ADC2_SQR1 ( read-write )  \ regular sequence register 1
    ADC2 $30 + constant ADC2_SQR2 ( read-write )  \ regular sequence register 2
    ADC2 $34 + constant ADC2_SQR3 ( read-write )  \ regular sequence register 3
    ADC2 $38 + constant ADC2_JSQR ( read-write )  \ injected sequence register
    ADC2 $3C + constant ADC2_JDR1 ( read-only )  \ injected data register x
    ADC2 $40 + constant ADC2_JDR2 ( read-only )  \ injected data register x
    ADC2 $44 + constant ADC2_JDR3 ( read-only )  \ injected data register x
    ADC2 $48 + constant ADC2_JDR4 ( read-only )  \ injected data register x
    ADC2 $4C + constant ADC2_DR ( read-only )  \ regular data register
    : ADC2_SR. cr ." ADC2_SR.  RW   $" ADC2_SR @ hex. ADC2_SR 1b. ;
    : ADC2_CR1. cr ." ADC2_CR1.  RW   $" ADC2_CR1 @ hex. ADC2_CR1 1b. ;
    : ADC2_CR2. cr ." ADC2_CR2.  RW   $" ADC2_CR2 @ hex. ADC2_CR2 1b. ;
    : ADC2_SMPR1. cr ." ADC2_SMPR1.  RW   $" ADC2_SMPR1 @ hex. ADC2_SMPR1 1b. ;
    : ADC2_SMPR2. cr ." ADC2_SMPR2.  RW   $" ADC2_SMPR2 @ hex. ADC2_SMPR2 1b. ;
    : ADC2_JOFR1. cr ." ADC2_JOFR1.  RW   $" ADC2_JOFR1 @ hex. ADC2_JOFR1 1b. ;
    : ADC2_JOFR2. cr ." ADC2_JOFR2.  RW   $" ADC2_JOFR2 @ hex. ADC2_JOFR2 1b. ;
    : ADC2_JOFR3. cr ." ADC2_JOFR3.  RW   $" ADC2_JOFR3 @ hex. ADC2_JOFR3 1b. ;
    : ADC2_JOFR4. cr ." ADC2_JOFR4.  RW   $" ADC2_JOFR4 @ hex. ADC2_JOFR4 1b. ;
    : ADC2_HTR. cr ." ADC2_HTR.  RW   $" ADC2_HTR @ hex. ADC2_HTR 1b. ;
    : ADC2_LTR. cr ." ADC2_LTR.  RW   $" ADC2_LTR @ hex. ADC2_LTR 1b. ;
    : ADC2_SQR1. cr ." ADC2_SQR1.  RW   $" ADC2_SQR1 @ hex. ADC2_SQR1 1b. ;
    : ADC2_SQR2. cr ." ADC2_SQR2.  RW   $" ADC2_SQR2 @ hex. ADC2_SQR2 1b. ;
    : ADC2_SQR3. cr ." ADC2_SQR3.  RW   $" ADC2_SQR3 @ hex. ADC2_SQR3 1b. ;
    : ADC2_JSQR. cr ." ADC2_JSQR.  RW   $" ADC2_JSQR @ hex. ADC2_JSQR 1b. ;
    : ADC2_JDR1. cr ." ADC2_JDR1.  RO   $" ADC2_JDR1 @ hex. ADC2_JDR1 1b. ;
    : ADC2_JDR2. cr ." ADC2_JDR2.  RO   $" ADC2_JDR2 @ hex. ADC2_JDR2 1b. ;
    : ADC2_JDR3. cr ." ADC2_JDR3.  RO   $" ADC2_JDR3 @ hex. ADC2_JDR3 1b. ;
    : ADC2_JDR4. cr ." ADC2_JDR4.  RO   $" ADC2_JDR4 @ hex. ADC2_JDR4 1b. ;
    : ADC2_DR. cr ." ADC2_DR.  RO   $" ADC2_DR @ hex. ADC2_DR 1b. ;
    : ADC2.
      ADC2_SR.
      ADC2_CR1.
      ADC2_CR2.
      ADC2_SMPR1.
      ADC2_SMPR2.
      ADC2_JOFR1.
      ADC2_JOFR2.
      ADC2_JOFR3.
      ADC2_JOFR4.
      ADC2_HTR.
      ADC2_LTR.
      ADC2_SQR1.
      ADC2_SQR2.
      ADC2_SQR3.
      ADC2_JSQR.
      ADC2_JDR1.
      ADC2_JDR2.
      ADC2_JDR3.
      ADC2_JDR4.
      ADC2_DR.
    ;
  [then]

  execute-defined? use-ADC3 [if]
    $40012200 constant ADC3 ( Analog-to-digital converter ) 
    ADC3 $0 + constant ADC3_SR ( read-write )  \ status register
    ADC3 $4 + constant ADC3_CR1 ( read-write )  \ control register 1
    ADC3 $8 + constant ADC3_CR2 ( read-write )  \ control register 2
    ADC3 $C + constant ADC3_SMPR1 ( read-write )  \ sample time register 1
    ADC3 $10 + constant ADC3_SMPR2 ( read-write )  \ sample time register 2
    ADC3 $14 + constant ADC3_JOFR1 ( read-write )  \ injected channel data offset register x
    ADC3 $18 + constant ADC3_JOFR2 ( read-write )  \ injected channel data offset register x
    ADC3 $1C + constant ADC3_JOFR3 ( read-write )  \ injected channel data offset register x
    ADC3 $20 + constant ADC3_JOFR4 ( read-write )  \ injected channel data offset register x
    ADC3 $24 + constant ADC3_HTR ( read-write )  \ watchdog higher threshold register
    ADC3 $28 + constant ADC3_LTR ( read-write )  \ watchdog lower threshold register
    ADC3 $2C + constant ADC3_SQR1 ( read-write )  \ regular sequence register 1
    ADC3 $30 + constant ADC3_SQR2 ( read-write )  \ regular sequence register 2
    ADC3 $34 + constant ADC3_SQR3 ( read-write )  \ regular sequence register 3
    ADC3 $38 + constant ADC3_JSQR ( read-write )  \ injected sequence register
    ADC3 $3C + constant ADC3_JDR1 ( read-only )  \ injected data register x
    ADC3 $40 + constant ADC3_JDR2 ( read-only )  \ injected data register x
    ADC3 $44 + constant ADC3_JDR3 ( read-only )  \ injected data register x
    ADC3 $48 + constant ADC3_JDR4 ( read-only )  \ injected data register x
    ADC3 $4C + constant ADC3_DR ( read-only )  \ regular data register
    : ADC3_SR. cr ." ADC3_SR.  RW   $" ADC3_SR @ hex. ADC3_SR 1b. ;
    : ADC3_CR1. cr ." ADC3_CR1.  RW   $" ADC3_CR1 @ hex. ADC3_CR1 1b. ;
    : ADC3_CR2. cr ." ADC3_CR2.  RW   $" ADC3_CR2 @ hex. ADC3_CR2 1b. ;
    : ADC3_SMPR1. cr ." ADC3_SMPR1.  RW   $" ADC3_SMPR1 @ hex. ADC3_SMPR1 1b. ;
    : ADC3_SMPR2. cr ." ADC3_SMPR2.  RW   $" ADC3_SMPR2 @ hex. ADC3_SMPR2 1b. ;
    : ADC3_JOFR1. cr ." ADC3_JOFR1.  RW   $" ADC3_JOFR1 @ hex. ADC3_JOFR1 1b. ;
    : ADC3_JOFR2. cr ." ADC3_JOFR2.  RW   $" ADC3_JOFR2 @ hex. ADC3_JOFR2 1b. ;
    : ADC3_JOFR3. cr ." ADC3_JOFR3.  RW   $" ADC3_JOFR3 @ hex. ADC3_JOFR3 1b. ;
    : ADC3_JOFR4. cr ." ADC3_JOFR4.  RW   $" ADC3_JOFR4 @ hex. ADC3_JOFR4 1b. ;
    : ADC3_HTR. cr ." ADC3_HTR.  RW   $" ADC3_HTR @ hex. ADC3_HTR 1b. ;
    : ADC3_LTR. cr ." ADC3_LTR.  RW   $" ADC3_LTR @ hex. ADC3_LTR 1b. ;
    : ADC3_SQR1. cr ." ADC3_SQR1.  RW   $" ADC3_SQR1 @ hex. ADC3_SQR1 1b. ;
    : ADC3_SQR2. cr ." ADC3_SQR2.  RW   $" ADC3_SQR2 @ hex. ADC3_SQR2 1b. ;
    : ADC3_SQR3. cr ." ADC3_SQR3.  RW   $" ADC3_SQR3 @ hex. ADC3_SQR3 1b. ;
    : ADC3_JSQR. cr ." ADC3_JSQR.  RW   $" ADC3_JSQR @ hex. ADC3_JSQR 1b. ;
    : ADC3_JDR1. cr ." ADC3_JDR1.  RO   $" ADC3_JDR1 @ hex. ADC3_JDR1 1b. ;
    : ADC3_JDR2. cr ." ADC3_JDR2.  RO   $" ADC3_JDR2 @ hex. ADC3_JDR2 1b. ;
    : ADC3_JDR3. cr ." ADC3_JDR3.  RO   $" ADC3_JDR3 @ hex. ADC3_JDR3 1b. ;
    : ADC3_JDR4. cr ." ADC3_JDR4.  RO   $" ADC3_JDR4 @ hex. ADC3_JDR4 1b. ;
    : ADC3_DR. cr ." ADC3_DR.  RO   $" ADC3_DR @ hex. ADC3_DR 1b. ;
    : ADC3.
      ADC3_SR.
      ADC3_CR1.
      ADC3_CR2.
      ADC3_SMPR1.
      ADC3_SMPR2.
      ADC3_JOFR1.
      ADC3_JOFR2.
      ADC3_JOFR3.
      ADC3_JOFR4.
      ADC3_HTR.
      ADC3_LTR.
      ADC3_SQR1.
      ADC3_SQR2.
      ADC3_SQR3.
      ADC3_JSQR.
      ADC3_JDR1.
      ADC3_JDR2.
      ADC3_JDR3.
      ADC3_JDR4.
      ADC3_DR.
    ;
  [then]

  execute-defined? use-DAC [if]
    $40007400 constant DAC ( Digital-to-analog converter ) 
    DAC $0 + constant DAC_CR ( read-write )  \ control register
    DAC $4 + constant DAC_SWTRIGR ( write-only )  \ software trigger register
    DAC $8 + constant DAC_DHR12R1 ( read-write )  \ channel1 12-bit right-aligned data holding register
    DAC $C + constant DAC_DHR12L1 ( read-write )  \ channel1 12-bit left aligned data holding register
    DAC $10 + constant DAC_DHR8R1 ( read-write )  \ channel1 8-bit right aligned data holding register
    DAC $14 + constant DAC_DHR12R2 ( read-write )  \ channel2 12-bit right aligned data holding register
    DAC $18 + constant DAC_DHR12L2 ( read-write )  \ channel2 12-bit left aligned data holding register
    DAC $1C + constant DAC_DHR8R2 ( read-write )  \ channel2 8-bit right-aligned data holding register
    DAC $20 + constant DAC_DHR12RD ( read-write )  \ Dual DAC 12-bit right-aligned data holding register
    DAC $24 + constant DAC_DHR12LD ( read-write )  \ DUAL DAC 12-bit left aligned data holding register
    DAC $28 + constant DAC_DHR8RD ( read-write )  \ DUAL DAC 8-bit right aligned data holding register
    DAC $2C + constant DAC_DOR1 ( read-only )  \ channel1 data output register
    DAC $30 + constant DAC_DOR2 ( read-only )  \ channel2 data output register
    DAC $34 + constant DAC_SR ( read-write )  \ status register
    : DAC_CR. cr ." DAC_CR.  RW   $" DAC_CR @ hex. DAC_CR 1b. ;
    : DAC_SWTRIGR. cr ." DAC_SWTRIGR " WRITEONLY ; 
    : DAC_DHR12R1. cr ." DAC_DHR12R1.  RW   $" DAC_DHR12R1 @ hex. DAC_DHR12R1 1b. ;
    : DAC_DHR12L1. cr ." DAC_DHR12L1.  RW   $" DAC_DHR12L1 @ hex. DAC_DHR12L1 1b. ;
    : DAC_DHR8R1. cr ." DAC_DHR8R1.  RW   $" DAC_DHR8R1 @ hex. DAC_DHR8R1 1b. ;
    : DAC_DHR12R2. cr ." DAC_DHR12R2.  RW   $" DAC_DHR12R2 @ hex. DAC_DHR12R2 1b. ;
    : DAC_DHR12L2. cr ." DAC_DHR12L2.  RW   $" DAC_DHR12L2 @ hex. DAC_DHR12L2 1b. ;
    : DAC_DHR8R2. cr ." DAC_DHR8R2.  RW   $" DAC_DHR8R2 @ hex. DAC_DHR8R2 1b. ;
    : DAC_DHR12RD. cr ." DAC_DHR12RD.  RW   $" DAC_DHR12RD @ hex. DAC_DHR12RD 1b. ;
    : DAC_DHR12LD. cr ." DAC_DHR12LD.  RW   $" DAC_DHR12LD @ hex. DAC_DHR12LD 1b. ;
    : DAC_DHR8RD. cr ." DAC_DHR8RD.  RW   $" DAC_DHR8RD @ hex. DAC_DHR8RD 1b. ;
    : DAC_DOR1. cr ." DAC_DOR1.  RO   $" DAC_DOR1 @ hex. DAC_DOR1 1b. ;
    : DAC_DOR2. cr ." DAC_DOR2.  RO   $" DAC_DOR2 @ hex. DAC_DOR2 1b. ;
    : DAC_SR. cr ." DAC_SR.  RW   $" DAC_SR @ hex. DAC_SR 1b. ;
    : DAC.
      DAC_CR.
      DAC_SWTRIGR.
      DAC_DHR12R1.
      DAC_DHR12L1.
      DAC_DHR8R1.
      DAC_DHR12R2.
      DAC_DHR12L2.
      DAC_DHR8R2.
      DAC_DHR12RD.
      DAC_DHR12LD.
      DAC_DHR8RD.
      DAC_DOR1.
      DAC_DOR2.
      DAC_SR.
    ;
  [then]

  execute-defined? use-PWR [if]
    $40007000 constant PWR ( Power control ) 
    PWR $0 + constant PWR_CR1 ( read-write )  \ power control register
    PWR $4 + constant PWR_CSR1 (  )  \ power control/status register
    PWR $8 + constant PWR_CR2 (  )  \ power control register
    PWR $C + constant PWR_CSR2 (  )  \ power control/status register
    : PWR_CR1. cr ." PWR_CR1.  RW   $" PWR_CR1 @ hex. PWR_CR1 1b. ;
    : PWR_CSR1. cr ." PWR_CSR1.   $" PWR_CSR1 @ hex. PWR_CSR1 1b. ;
    : PWR_CR2. cr ." PWR_CR2.   $" PWR_CR2 @ hex. PWR_CR2 1b. ;
    : PWR_CSR2. cr ." PWR_CSR2.   $" PWR_CSR2 @ hex. PWR_CSR2 1b. ;
    : PWR.
      PWR_CR1.
      PWR_CSR1.
      PWR_CR2.
      PWR_CSR2.
    ;
  [then]

  execute-defined? use-IWDG [if]
    $40003000 constant IWDG ( Independent watchdog ) 
    IWDG $0 + constant IWDG_KR ( write-only )  \ Key register
    IWDG $4 + constant IWDG_PR ( read-write )  \ Prescaler register
    IWDG $8 + constant IWDG_RLR ( read-write )  \ Reload register
    IWDG $C + constant IWDG_SR ( read-only )  \ Status register
    IWDG $10 + constant IWDG_WINR ( read-write )  \ Window register
    : IWDG_KR. cr ." IWDG_KR " WRITEONLY ; 
    : IWDG_PR. cr ." IWDG_PR.  RW   $" IWDG_PR @ hex. IWDG_PR 1b. ;
    : IWDG_RLR. cr ." IWDG_RLR.  RW   $" IWDG_RLR @ hex. IWDG_RLR 1b. ;
    : IWDG_SR. cr ." IWDG_SR.  RO   $" IWDG_SR @ hex. IWDG_SR 1b. ;
    : IWDG_WINR. cr ." IWDG_WINR.  RW   $" IWDG_WINR @ hex. IWDG_WINR 1b. ;
    : IWDG.
      IWDG_KR.
      IWDG_PR.
      IWDG_RLR.
      IWDG_SR.
      IWDG_WINR.
    ;
  [then]

  execute-defined? use-WWDG [if]
    $40002C00 constant WWDG ( Window watchdog ) 
    WWDG $0 + constant WWDG_CR ( read-write )  \ Control register
    WWDG $4 + constant WWDG_CFR ( read-write )  \ Configuration register
    WWDG $8 + constant WWDG_SR ( read-write )  \ Status register
    : WWDG_CR. cr ." WWDG_CR.  RW   $" WWDG_CR @ hex. WWDG_CR 1b. ;
    : WWDG_CFR. cr ." WWDG_CFR.  RW   $" WWDG_CFR @ hex. WWDG_CFR 1b. ;
    : WWDG_SR. cr ." WWDG_SR.  RW   $" WWDG_SR @ hex. WWDG_SR 1b. ;
    : WWDG.
      WWDG_CR.
      WWDG_CFR.
      WWDG_SR.
    ;
  [then]

  execute-defined? use-C_ADC [if]
    $40012300 constant C_ADC ( Common ADC registers ) 
    C_ADC $0 + constant C_ADC_CSR ( read-only )  \ ADC Common status register
    C_ADC $4 + constant C_ADC_CCR ( read-write )  \ ADC common control register
    C_ADC $8 + constant C_ADC_CDR ( read-only )  \ ADC common regular data register for dual and triple modes
    : C_ADC_CSR. cr ." C_ADC_CSR.  RO   $" C_ADC_CSR @ hex. C_ADC_CSR 1b. ;
    : C_ADC_CCR. cr ." C_ADC_CCR.  RW   $" C_ADC_CCR @ hex. C_ADC_CCR 1b. ;
    : C_ADC_CDR. cr ." C_ADC_CDR.  RO   $" C_ADC_CDR @ hex. C_ADC_CDR 1b. ;
    : C_ADC.
      C_ADC_CSR.
      C_ADC_CCR.
      C_ADC_CDR.
    ;
  [then]

  execute-defined? use-TIM1 [if]
    $40010000 constant TIM1 ( Advanced-timers ) 
    TIM1 $0 + constant TIM1_CR1 ( read-write )  \ control register 1
    TIM1 $4 + constant TIM1_CR2 ( read-write )  \ control register 2
    TIM1 $8 + constant TIM1_SMCR ( read-write )  \ slave mode control register
    TIM1 $C + constant TIM1_DIER ( read-write )  \ DMA/Interrupt enable register
    TIM1 $10 + constant TIM1_SR ( read-write )  \ status register
    TIM1 $14 + constant TIM1_EGR ( write-only )  \ event generation register
    TIM1 $18 + constant TIM1_CCMR1_Output ( read-write )  \ capture/compare mode register 1 output mode
    TIM1 $18 + constant TIM1_CCMR1_Input ( read-write )  \ capture/compare mode register 1 input mode
    TIM1 $1C + constant TIM1_CCMR2_Output ( read-write )  \ capture/compare mode register 2 output mode
    TIM1 $1C + constant TIM1_CCMR2_Input ( read-write )  \ capture/compare mode register 2 input mode
    TIM1 $20 + constant TIM1_CCER ( read-write )  \ capture/compare enable register
    TIM1 $24 + constant TIM1_CNT ( read-write )  \ counter
    TIM1 $28 + constant TIM1_PSC ( read-write )  \ prescaler
    TIM1 $2C + constant TIM1_ARR ( read-write )  \ auto-reload register
    TIM1 $34 + constant TIM1_CCR1 ( read-write )  \ capture/compare register 1
    TIM1 $38 + constant TIM1_CCR2 ( read-write )  \ capture/compare register 2
    TIM1 $3C + constant TIM1_CCR3 ( read-write )  \ capture/compare register 3
    TIM1 $40 + constant TIM1_CCR4 ( read-write )  \ capture/compare register 4
    TIM1 $48 + constant TIM1_DCR ( read-write )  \ DMA control register
    TIM1 $4C + constant TIM1_DMAR ( read-write )  \ DMA address for full transfer
    TIM1 $30 + constant TIM1_RCR ( read-write )  \ repetition counter register
    TIM1 $44 + constant TIM1_BDTR ( read-write )  \ break and dead-time register
    TIM1 $54 + constant TIM1_CCMR3_Output ( read-write )  \ capture/compare mode register 3 output mode
    TIM1 $58 + constant TIM1_CCR5 ( read-write )  \ capture/compare register 5
    TIM1 $5C + constant TIM1_CRR6 ( read-write )  \ capture/compare register 6
    : TIM1_CR1. cr ." TIM1_CR1.  RW   $" TIM1_CR1 @ hex. TIM1_CR1 1b. ;
    : TIM1_CR2. cr ." TIM1_CR2.  RW   $" TIM1_CR2 @ hex. TIM1_CR2 1b. ;
    : TIM1_SMCR. cr ." TIM1_SMCR.  RW   $" TIM1_SMCR @ hex. TIM1_SMCR 1b. ;
    : TIM1_DIER. cr ." TIM1_DIER.  RW   $" TIM1_DIER @ hex. TIM1_DIER 1b. ;
    : TIM1_SR. cr ." TIM1_SR.  RW   $" TIM1_SR @ hex. TIM1_SR 1b. ;
    : TIM1_EGR. cr ." TIM1_EGR " WRITEONLY ; 
    : TIM1_CCMR1_Output. cr ." TIM1_CCMR1_Output.  RW   $" TIM1_CCMR1_Output @ hex. TIM1_CCMR1_Output 1b. ;
    : TIM1_CCMR1_Input. cr ." TIM1_CCMR1_Input.  RW   $" TIM1_CCMR1_Input @ hex. TIM1_CCMR1_Input 1b. ;
    : TIM1_CCMR2_Output. cr ." TIM1_CCMR2_Output.  RW   $" TIM1_CCMR2_Output @ hex. TIM1_CCMR2_Output 1b. ;
    : TIM1_CCMR2_Input. cr ." TIM1_CCMR2_Input.  RW   $" TIM1_CCMR2_Input @ hex. TIM1_CCMR2_Input 1b. ;
    : TIM1_CCER. cr ." TIM1_CCER.  RW   $" TIM1_CCER @ hex. TIM1_CCER 1b. ;
    : TIM1_CNT. cr ." TIM1_CNT.  RW   $" TIM1_CNT @ hex. TIM1_CNT 1b. ;
    : TIM1_PSC. cr ." TIM1_PSC.  RW   $" TIM1_PSC @ hex. TIM1_PSC 1b. ;
    : TIM1_ARR. cr ." TIM1_ARR.  RW   $" TIM1_ARR @ hex. TIM1_ARR 1b. ;
    : TIM1_CCR1. cr ." TIM1_CCR1.  RW   $" TIM1_CCR1 @ hex. TIM1_CCR1 1b. ;
    : TIM1_CCR2. cr ." TIM1_CCR2.  RW   $" TIM1_CCR2 @ hex. TIM1_CCR2 1b. ;
    : TIM1_CCR3. cr ." TIM1_CCR3.  RW   $" TIM1_CCR3 @ hex. TIM1_CCR3 1b. ;
    : TIM1_CCR4. cr ." TIM1_CCR4.  RW   $" TIM1_CCR4 @ hex. TIM1_CCR4 1b. ;
    : TIM1_DCR. cr ." TIM1_DCR.  RW   $" TIM1_DCR @ hex. TIM1_DCR 1b. ;
    : TIM1_DMAR. cr ." TIM1_DMAR.  RW   $" TIM1_DMAR @ hex. TIM1_DMAR 1b. ;
    : TIM1_RCR. cr ." TIM1_RCR.  RW   $" TIM1_RCR @ hex. TIM1_RCR 1b. ;
    : TIM1_BDTR. cr ." TIM1_BDTR.  RW   $" TIM1_BDTR @ hex. TIM1_BDTR 1b. ;
    : TIM1_CCMR3_Output. cr ." TIM1_CCMR3_Output.  RW   $" TIM1_CCMR3_Output @ hex. TIM1_CCMR3_Output 1b. ;
    : TIM1_CCR5. cr ." TIM1_CCR5.  RW   $" TIM1_CCR5 @ hex. TIM1_CCR5 1b. ;
    : TIM1_CRR6. cr ." TIM1_CRR6.  RW   $" TIM1_CRR6 @ hex. TIM1_CRR6 1b. ;
    : TIM1.
      TIM1_CR1.
      TIM1_CR2.
      TIM1_SMCR.
      TIM1_DIER.
      TIM1_SR.
      TIM1_EGR.
      TIM1_CCMR1_Output.
      TIM1_CCMR1_Input.
      TIM1_CCMR2_Output.
      TIM1_CCMR2_Input.
      TIM1_CCER.
      TIM1_CNT.
      TIM1_PSC.
      TIM1_ARR.
      TIM1_CCR1.
      TIM1_CCR2.
      TIM1_CCR3.
      TIM1_CCR4.
      TIM1_DCR.
      TIM1_DMAR.
      TIM1_RCR.
      TIM1_BDTR.
      TIM1_CCMR3_Output.
      TIM1_CCR5.
      TIM1_CRR6.
    ;
  [then]

  execute-defined? use-TIM8 [if]
    $40010400 constant TIM8 ( Advanced-timers ) 
    TIM8 $0 + constant TIM8_CR1 ( read-write )  \ control register 1
    TIM8 $4 + constant TIM8_CR2 ( read-write )  \ control register 2
    TIM8 $8 + constant TIM8_SMCR ( read-write )  \ slave mode control register
    TIM8 $C + constant TIM8_DIER ( read-write )  \ DMA/Interrupt enable register
    TIM8 $10 + constant TIM8_SR ( read-write )  \ status register
    TIM8 $14 + constant TIM8_EGR ( write-only )  \ event generation register
    TIM8 $18 + constant TIM8_CCMR1_Output ( read-write )  \ capture/compare mode register 1 output mode
    TIM8 $18 + constant TIM8_CCMR1_Input ( read-write )  \ capture/compare mode register 1 input mode
    TIM8 $1C + constant TIM8_CCMR2_Output ( read-write )  \ capture/compare mode register 2 output mode
    TIM8 $1C + constant TIM8_CCMR2_Input ( read-write )  \ capture/compare mode register 2 input mode
    TIM8 $20 + constant TIM8_CCER ( read-write )  \ capture/compare enable register
    TIM8 $24 + constant TIM8_CNT ( read-write )  \ counter
    TIM8 $28 + constant TIM8_PSC ( read-write )  \ prescaler
    TIM8 $2C + constant TIM8_ARR ( read-write )  \ auto-reload register
    TIM8 $34 + constant TIM8_CCR1 ( read-write )  \ capture/compare register 1
    TIM8 $38 + constant TIM8_CCR2 ( read-write )  \ capture/compare register 2
    TIM8 $3C + constant TIM8_CCR3 ( read-write )  \ capture/compare register 3
    TIM8 $40 + constant TIM8_CCR4 ( read-write )  \ capture/compare register 4
    TIM8 $48 + constant TIM8_DCR ( read-write )  \ DMA control register
    TIM8 $4C + constant TIM8_DMAR ( read-write )  \ DMA address for full transfer
    TIM8 $30 + constant TIM8_RCR ( read-write )  \ repetition counter register
    TIM8 $44 + constant TIM8_BDTR ( read-write )  \ break and dead-time register
    TIM8 $54 + constant TIM8_CCMR3_Output ( read-write )  \ capture/compare mode register 3 output mode
    TIM8 $58 + constant TIM8_CCR5 ( read-write )  \ capture/compare register 5
    TIM8 $5C + constant TIM8_CRR6 ( read-write )  \ capture/compare register 6
    : TIM8_CR1. cr ." TIM8_CR1.  RW   $" TIM8_CR1 @ hex. TIM8_CR1 1b. ;
    : TIM8_CR2. cr ." TIM8_CR2.  RW   $" TIM8_CR2 @ hex. TIM8_CR2 1b. ;
    : TIM8_SMCR. cr ." TIM8_SMCR.  RW   $" TIM8_SMCR @ hex. TIM8_SMCR 1b. ;
    : TIM8_DIER. cr ." TIM8_DIER.  RW   $" TIM8_DIER @ hex. TIM8_DIER 1b. ;
    : TIM8_SR. cr ." TIM8_SR.  RW   $" TIM8_SR @ hex. TIM8_SR 1b. ;
    : TIM8_EGR. cr ." TIM8_EGR " WRITEONLY ; 
    : TIM8_CCMR1_Output. cr ." TIM8_CCMR1_Output.  RW   $" TIM8_CCMR1_Output @ hex. TIM8_CCMR1_Output 1b. ;
    : TIM8_CCMR1_Input. cr ." TIM8_CCMR1_Input.  RW   $" TIM8_CCMR1_Input @ hex. TIM8_CCMR1_Input 1b. ;
    : TIM8_CCMR2_Output. cr ." TIM8_CCMR2_Output.  RW   $" TIM8_CCMR2_Output @ hex. TIM8_CCMR2_Output 1b. ;
    : TIM8_CCMR2_Input. cr ." TIM8_CCMR2_Input.  RW   $" TIM8_CCMR2_Input @ hex. TIM8_CCMR2_Input 1b. ;
    : TIM8_CCER. cr ." TIM8_CCER.  RW   $" TIM8_CCER @ hex. TIM8_CCER 1b. ;
    : TIM8_CNT. cr ." TIM8_CNT.  RW   $" TIM8_CNT @ hex. TIM8_CNT 1b. ;
    : TIM8_PSC. cr ." TIM8_PSC.  RW   $" TIM8_PSC @ hex. TIM8_PSC 1b. ;
    : TIM8_ARR. cr ." TIM8_ARR.  RW   $" TIM8_ARR @ hex. TIM8_ARR 1b. ;
    : TIM8_CCR1. cr ." TIM8_CCR1.  RW   $" TIM8_CCR1 @ hex. TIM8_CCR1 1b. ;
    : TIM8_CCR2. cr ." TIM8_CCR2.  RW   $" TIM8_CCR2 @ hex. TIM8_CCR2 1b. ;
    : TIM8_CCR3. cr ." TIM8_CCR3.  RW   $" TIM8_CCR3 @ hex. TIM8_CCR3 1b. ;
    : TIM8_CCR4. cr ." TIM8_CCR4.  RW   $" TIM8_CCR4 @ hex. TIM8_CCR4 1b. ;
    : TIM8_DCR. cr ." TIM8_DCR.  RW   $" TIM8_DCR @ hex. TIM8_DCR 1b. ;
    : TIM8_DMAR. cr ." TIM8_DMAR.  RW   $" TIM8_DMAR @ hex. TIM8_DMAR 1b. ;
    : TIM8_RCR. cr ." TIM8_RCR.  RW   $" TIM8_RCR @ hex. TIM8_RCR 1b. ;
    : TIM8_BDTR. cr ." TIM8_BDTR.  RW   $" TIM8_BDTR @ hex. TIM8_BDTR 1b. ;
    : TIM8_CCMR3_Output. cr ." TIM8_CCMR3_Output.  RW   $" TIM8_CCMR3_Output @ hex. TIM8_CCMR3_Output 1b. ;
    : TIM8_CCR5. cr ." TIM8_CCR5.  RW   $" TIM8_CCR5 @ hex. TIM8_CCR5 1b. ;
    : TIM8_CRR6. cr ." TIM8_CRR6.  RW   $" TIM8_CRR6 @ hex. TIM8_CRR6 1b. ;
    : TIM8.
      TIM8_CR1.
      TIM8_CR2.
      TIM8_SMCR.
      TIM8_DIER.
      TIM8_SR.
      TIM8_EGR.
      TIM8_CCMR1_Output.
      TIM8_CCMR1_Input.
      TIM8_CCMR2_Output.
      TIM8_CCMR2_Input.
      TIM8_CCER.
      TIM8_CNT.
      TIM8_PSC.
      TIM8_ARR.
      TIM8_CCR1.
      TIM8_CCR2.
      TIM8_CCR3.
      TIM8_CCR4.
      TIM8_DCR.
      TIM8_DMAR.
      TIM8_RCR.
      TIM8_BDTR.
      TIM8_CCMR3_Output.
      TIM8_CCR5.
      TIM8_CRR6.
    ;
  [then]

  execute-defined? use-TIM2 [if]
    $40000000 constant TIM2 ( General purpose timers ) 
    TIM2 $0 + constant TIM2_CR1 ( read-write )  \ control register 1
    TIM2 $4 + constant TIM2_CR2 ( read-write )  \ control register 2
    TIM2 $8 + constant TIM2_SMCR ( read-write )  \ slave mode control register
    TIM2 $C + constant TIM2_DIER ( read-write )  \ DMA/Interrupt enable register
    TIM2 $10 + constant TIM2_SR ( read-write )  \ status register
    TIM2 $14 + constant TIM2_EGR ( write-only )  \ event generation register
    TIM2 $18 + constant TIM2_CCMR1_Output ( read-write )  \ capture/compare mode register 1 output mode
    TIM2 $18 + constant TIM2_CCMR1_Input ( read-write )  \ capture/compare mode register 1 input mode
    TIM2 $1C + constant TIM2_CCMR2_Output ( read-write )  \ capture/compare mode register 2 output mode
    TIM2 $1C + constant TIM2_CCMR2_Input ( read-write )  \ capture/compare mode register 2 input mode
    TIM2 $20 + constant TIM2_CCER ( read-write )  \ capture/compare enable register
    TIM2 $24 + constant TIM2_CNT ( read-write )  \ counter
    TIM2 $28 + constant TIM2_PSC ( read-write )  \ prescaler
    TIM2 $2C + constant TIM2_ARR ( read-write )  \ auto-reload register
    TIM2 $34 + constant TIM2_CCR1 ( read-write )  \ capture/compare register 1
    TIM2 $38 + constant TIM2_CCR2 ( read-write )  \ capture/compare register 2
    TIM2 $3C + constant TIM2_CCR3 ( read-write )  \ capture/compare register 3
    TIM2 $40 + constant TIM2_CCR4 ( read-write )  \ capture/compare register 4
    TIM2 $48 + constant TIM2_DCR ( read-write )  \ DMA control register
    TIM2 $4C + constant TIM2_DMAR ( read-write )  \ DMA address for full transfer
    TIM2 $50 + constant TIM2_OR1 ( read-write )  \ TIM2 option register 1
    TIM2 $60 + constant TIM2_OR2 ( read-write )  \ TIM2 option register 2
    : TIM2_CR1. cr ." TIM2_CR1.  RW   $" TIM2_CR1 @ hex. TIM2_CR1 1b. ;
    : TIM2_CR2. cr ." TIM2_CR2.  RW   $" TIM2_CR2 @ hex. TIM2_CR2 1b. ;
    : TIM2_SMCR. cr ." TIM2_SMCR.  RW   $" TIM2_SMCR @ hex. TIM2_SMCR 1b. ;
    : TIM2_DIER. cr ." TIM2_DIER.  RW   $" TIM2_DIER @ hex. TIM2_DIER 1b. ;
    : TIM2_SR. cr ." TIM2_SR.  RW   $" TIM2_SR @ hex. TIM2_SR 1b. ;
    : TIM2_EGR. cr ." TIM2_EGR " WRITEONLY ; 
    : TIM2_CCMR1_Output. cr ." TIM2_CCMR1_Output.  RW   $" TIM2_CCMR1_Output @ hex. TIM2_CCMR1_Output 1b. ;
    : TIM2_CCMR1_Input. cr ." TIM2_CCMR1_Input.  RW   $" TIM2_CCMR1_Input @ hex. TIM2_CCMR1_Input 1b. ;
    : TIM2_CCMR2_Output. cr ." TIM2_CCMR2_Output.  RW   $" TIM2_CCMR2_Output @ hex. TIM2_CCMR2_Output 1b. ;
    : TIM2_CCMR2_Input. cr ." TIM2_CCMR2_Input.  RW   $" TIM2_CCMR2_Input @ hex. TIM2_CCMR2_Input 1b. ;
    : TIM2_CCER. cr ." TIM2_CCER.  RW   $" TIM2_CCER @ hex. TIM2_CCER 1b. ;
    : TIM2_CNT. cr ." TIM2_CNT.  RW   $" TIM2_CNT @ hex. TIM2_CNT 1b. ;
    : TIM2_PSC. cr ." TIM2_PSC.  RW   $" TIM2_PSC @ hex. TIM2_PSC 1b. ;
    : TIM2_ARR. cr ." TIM2_ARR.  RW   $" TIM2_ARR @ hex. TIM2_ARR 1b. ;
    : TIM2_CCR1. cr ." TIM2_CCR1.  RW   $" TIM2_CCR1 @ hex. TIM2_CCR1 1b. ;
    : TIM2_CCR2. cr ." TIM2_CCR2.  RW   $" TIM2_CCR2 @ hex. TIM2_CCR2 1b. ;
    : TIM2_CCR3. cr ." TIM2_CCR3.  RW   $" TIM2_CCR3 @ hex. TIM2_CCR3 1b. ;
    : TIM2_CCR4. cr ." TIM2_CCR4.  RW   $" TIM2_CCR4 @ hex. TIM2_CCR4 1b. ;
    : TIM2_DCR. cr ." TIM2_DCR.  RW   $" TIM2_DCR @ hex. TIM2_DCR 1b. ;
    : TIM2_DMAR. cr ." TIM2_DMAR.  RW   $" TIM2_DMAR @ hex. TIM2_DMAR 1b. ;
    : TIM2_OR1. cr ." TIM2_OR1.  RW   $" TIM2_OR1 @ hex. TIM2_OR1 1b. ;
    : TIM2_OR2. cr ." TIM2_OR2.  RW   $" TIM2_OR2 @ hex. TIM2_OR2 1b. ;
    : TIM2.
      TIM2_CR1.
      TIM2_CR2.
      TIM2_SMCR.
      TIM2_DIER.
      TIM2_SR.
      TIM2_EGR.
      TIM2_CCMR1_Output.
      TIM2_CCMR1_Input.
      TIM2_CCMR2_Output.
      TIM2_CCMR2_Input.
      TIM2_CCER.
      TIM2_CNT.
      TIM2_PSC.
      TIM2_ARR.
      TIM2_CCR1.
      TIM2_CCR2.
      TIM2_CCR3.
      TIM2_CCR4.
      TIM2_DCR.
      TIM2_DMAR.
      TIM2_OR1.
      TIM2_OR2.
    ;
  [then]

  execute-defined? use-TIM3 [if]
    $40000400 constant TIM3 ( General purpose timers ) 
    TIM3 $0 + constant TIM3_CR1 ( read-write )  \ control register 1
    TIM3 $4 + constant TIM3_CR2 ( read-write )  \ control register 2
    TIM3 $8 + constant TIM3_SMCR ( read-write )  \ slave mode control register
    TIM3 $C + constant TIM3_DIER ( read-write )  \ DMA/Interrupt enable register
    TIM3 $10 + constant TIM3_SR ( read-write )  \ status register
    TIM3 $14 + constant TIM3_EGR ( write-only )  \ event generation register
    TIM3 $18 + constant TIM3_CCMR1_Output ( read-write )  \ capture/compare mode register 1 output mode
    TIM3 $18 + constant TIM3_CCMR1_Input ( read-write )  \ capture/compare mode register 1 input mode
    TIM3 $1C + constant TIM3_CCMR2_Output ( read-write )  \ capture/compare mode register 2 output mode
    TIM3 $1C + constant TIM3_CCMR2_Input ( read-write )  \ capture/compare mode register 2 input mode
    TIM3 $20 + constant TIM3_CCER ( read-write )  \ capture/compare enable register
    TIM3 $24 + constant TIM3_CNT ( read-write )  \ counter
    TIM3 $28 + constant TIM3_PSC ( read-write )  \ prescaler
    TIM3 $2C + constant TIM3_ARR ( read-write )  \ auto-reload register
    TIM3 $34 + constant TIM3_CCR1 ( read-write )  \ capture/compare register 1
    TIM3 $38 + constant TIM3_CCR2 ( read-write )  \ capture/compare register 2
    TIM3 $3C + constant TIM3_CCR3 ( read-write )  \ capture/compare register 3
    TIM3 $40 + constant TIM3_CCR4 ( read-write )  \ capture/compare register 4
    TIM3 $48 + constant TIM3_DCR ( read-write )  \ DMA control register
    TIM3 $4C + constant TIM3_DMAR ( read-write )  \ DMA address for full transfer
    TIM3 $50 + constant TIM3_OR1 ( read-write )  \ TIM3 option register 1
    TIM3 $60 + constant TIM3_OR2 ( read-write )  \ TIM3 option register 2
    : TIM3_CR1. cr ." TIM3_CR1.  RW   $" TIM3_CR1 @ hex. TIM3_CR1 1b. ;
    : TIM3_CR2. cr ." TIM3_CR2.  RW   $" TIM3_CR2 @ hex. TIM3_CR2 1b. ;
    : TIM3_SMCR. cr ." TIM3_SMCR.  RW   $" TIM3_SMCR @ hex. TIM3_SMCR 1b. ;
    : TIM3_DIER. cr ." TIM3_DIER.  RW   $" TIM3_DIER @ hex. TIM3_DIER 1b. ;
    : TIM3_SR. cr ." TIM3_SR.  RW   $" TIM3_SR @ hex. TIM3_SR 1b. ;
    : TIM3_EGR. cr ." TIM3_EGR " WRITEONLY ; 
    : TIM3_CCMR1_Output. cr ." TIM3_CCMR1_Output.  RW   $" TIM3_CCMR1_Output @ hex. TIM3_CCMR1_Output 1b. ;
    : TIM3_CCMR1_Input. cr ." TIM3_CCMR1_Input.  RW   $" TIM3_CCMR1_Input @ hex. TIM3_CCMR1_Input 1b. ;
    : TIM3_CCMR2_Output. cr ." TIM3_CCMR2_Output.  RW   $" TIM3_CCMR2_Output @ hex. TIM3_CCMR2_Output 1b. ;
    : TIM3_CCMR2_Input. cr ." TIM3_CCMR2_Input.  RW   $" TIM3_CCMR2_Input @ hex. TIM3_CCMR2_Input 1b. ;
    : TIM3_CCER. cr ." TIM3_CCER.  RW   $" TIM3_CCER @ hex. TIM3_CCER 1b. ;
    : TIM3_CNT. cr ." TIM3_CNT.  RW   $" TIM3_CNT @ hex. TIM3_CNT 1b. ;
    : TIM3_PSC. cr ." TIM3_PSC.  RW   $" TIM3_PSC @ hex. TIM3_PSC 1b. ;
    : TIM3_ARR. cr ." TIM3_ARR.  RW   $" TIM3_ARR @ hex. TIM3_ARR 1b. ;
    : TIM3_CCR1. cr ." TIM3_CCR1.  RW   $" TIM3_CCR1 @ hex. TIM3_CCR1 1b. ;
    : TIM3_CCR2. cr ." TIM3_CCR2.  RW   $" TIM3_CCR2 @ hex. TIM3_CCR2 1b. ;
    : TIM3_CCR3. cr ." TIM3_CCR3.  RW   $" TIM3_CCR3 @ hex. TIM3_CCR3 1b. ;
    : TIM3_CCR4. cr ." TIM3_CCR4.  RW   $" TIM3_CCR4 @ hex. TIM3_CCR4 1b. ;
    : TIM3_DCR. cr ." TIM3_DCR.  RW   $" TIM3_DCR @ hex. TIM3_DCR 1b. ;
    : TIM3_DMAR. cr ." TIM3_DMAR.  RW   $" TIM3_DMAR @ hex. TIM3_DMAR 1b. ;
    : TIM3_OR1. cr ." TIM3_OR1.  RW   $" TIM3_OR1 @ hex. TIM3_OR1 1b. ;
    : TIM3_OR2. cr ." TIM3_OR2.  RW   $" TIM3_OR2 @ hex. TIM3_OR2 1b. ;
    : TIM3.
      TIM3_CR1.
      TIM3_CR2.
      TIM3_SMCR.
      TIM3_DIER.
      TIM3_SR.
      TIM3_EGR.
      TIM3_CCMR1_Output.
      TIM3_CCMR1_Input.
      TIM3_CCMR2_Output.
      TIM3_CCMR2_Input.
      TIM3_CCER.
      TIM3_CNT.
      TIM3_PSC.
      TIM3_ARR.
      TIM3_CCR1.
      TIM3_CCR2.
      TIM3_CCR3.
      TIM3_CCR4.
      TIM3_DCR.
      TIM3_DMAR.
      TIM3_OR1.
      TIM3_OR2.
    ;
  [then]

  execute-defined? use-TIM4 [if]
    $40000800 constant TIM4 ( General purpose timers ) 
    TIM4 $0 + constant TIM4_CR1 ( read-write )  \ control register 1
    TIM4 $4 + constant TIM4_CR2 ( read-write )  \ control register 2
    TIM4 $8 + constant TIM4_SMCR ( read-write )  \ slave mode control register
    TIM4 $C + constant TIM4_DIER ( read-write )  \ DMA/Interrupt enable register
    TIM4 $10 + constant TIM4_SR ( read-write )  \ status register
    TIM4 $14 + constant TIM4_EGR ( write-only )  \ event generation register
    TIM4 $18 + constant TIM4_CCMR1_Output ( read-write )  \ capture/compare mode register 1 output mode
    TIM4 $18 + constant TIM4_CCMR1_Input ( read-write )  \ capture/compare mode register 1 input mode
    TIM4 $1C + constant TIM4_CCMR2_Output ( read-write )  \ capture/compare mode register 2 output mode
    TIM4 $1C + constant TIM4_CCMR2_Input ( read-write )  \ capture/compare mode register 2 input mode
    TIM4 $20 + constant TIM4_CCER ( read-write )  \ capture/compare enable register
    TIM4 $24 + constant TIM4_CNT ( read-write )  \ counter
    TIM4 $28 + constant TIM4_PSC ( read-write )  \ prescaler
    TIM4 $2C + constant TIM4_ARR ( read-write )  \ auto-reload register
    TIM4 $34 + constant TIM4_CCR1 ( read-write )  \ capture/compare register 1
    TIM4 $38 + constant TIM4_CCR2 ( read-write )  \ capture/compare register 2
    TIM4 $3C + constant TIM4_CCR3 ( read-write )  \ capture/compare register 3
    TIM4 $40 + constant TIM4_CCR4 ( read-write )  \ capture/compare register 4
    TIM4 $48 + constant TIM4_DCR ( read-write )  \ DMA control register
    TIM4 $4C + constant TIM4_DMAR ( read-write )  \ DMA address for full transfer
    : TIM4_CR1. cr ." TIM4_CR1.  RW   $" TIM4_CR1 @ hex. TIM4_CR1 1b. ;
    : TIM4_CR2. cr ." TIM4_CR2.  RW   $" TIM4_CR2 @ hex. TIM4_CR2 1b. ;
    : TIM4_SMCR. cr ." TIM4_SMCR.  RW   $" TIM4_SMCR @ hex. TIM4_SMCR 1b. ;
    : TIM4_DIER. cr ." TIM4_DIER.  RW   $" TIM4_DIER @ hex. TIM4_DIER 1b. ;
    : TIM4_SR. cr ." TIM4_SR.  RW   $" TIM4_SR @ hex. TIM4_SR 1b. ;
    : TIM4_EGR. cr ." TIM4_EGR " WRITEONLY ; 
    : TIM4_CCMR1_Output. cr ." TIM4_CCMR1_Output.  RW   $" TIM4_CCMR1_Output @ hex. TIM4_CCMR1_Output 1b. ;
    : TIM4_CCMR1_Input. cr ." TIM4_CCMR1_Input.  RW   $" TIM4_CCMR1_Input @ hex. TIM4_CCMR1_Input 1b. ;
    : TIM4_CCMR2_Output. cr ." TIM4_CCMR2_Output.  RW   $" TIM4_CCMR2_Output @ hex. TIM4_CCMR2_Output 1b. ;
    : TIM4_CCMR2_Input. cr ." TIM4_CCMR2_Input.  RW   $" TIM4_CCMR2_Input @ hex. TIM4_CCMR2_Input 1b. ;
    : TIM4_CCER. cr ." TIM4_CCER.  RW   $" TIM4_CCER @ hex. TIM4_CCER 1b. ;
    : TIM4_CNT. cr ." TIM4_CNT.  RW   $" TIM4_CNT @ hex. TIM4_CNT 1b. ;
    : TIM4_PSC. cr ." TIM4_PSC.  RW   $" TIM4_PSC @ hex. TIM4_PSC 1b. ;
    : TIM4_ARR. cr ." TIM4_ARR.  RW   $" TIM4_ARR @ hex. TIM4_ARR 1b. ;
    : TIM4_CCR1. cr ." TIM4_CCR1.  RW   $" TIM4_CCR1 @ hex. TIM4_CCR1 1b. ;
    : TIM4_CCR2. cr ." TIM4_CCR2.  RW   $" TIM4_CCR2 @ hex. TIM4_CCR2 1b. ;
    : TIM4_CCR3. cr ." TIM4_CCR3.  RW   $" TIM4_CCR3 @ hex. TIM4_CCR3 1b. ;
    : TIM4_CCR4. cr ." TIM4_CCR4.  RW   $" TIM4_CCR4 @ hex. TIM4_CCR4 1b. ;
    : TIM4_DCR. cr ." TIM4_DCR.  RW   $" TIM4_DCR @ hex. TIM4_DCR 1b. ;
    : TIM4_DMAR. cr ." TIM4_DMAR.  RW   $" TIM4_DMAR @ hex. TIM4_DMAR 1b. ;
    : TIM4.
      TIM4_CR1.
      TIM4_CR2.
      TIM4_SMCR.
      TIM4_DIER.
      TIM4_SR.
      TIM4_EGR.
      TIM4_CCMR1_Output.
      TIM4_CCMR1_Input.
      TIM4_CCMR2_Output.
      TIM4_CCMR2_Input.
      TIM4_CCER.
      TIM4_CNT.
      TIM4_PSC.
      TIM4_ARR.
      TIM4_CCR1.
      TIM4_CCR2.
      TIM4_CCR3.
      TIM4_CCR4.
      TIM4_DCR.
      TIM4_DMAR.
    ;
  [then]

  execute-defined? use-TIM5 [if]
    $40000C00 constant TIM5 ( General purpose timers ) 
    TIM5 $0 + constant TIM5_CR1 ( read-write )  \ control register 1
    TIM5 $4 + constant TIM5_CR2 ( read-write )  \ control register 2
    TIM5 $8 + constant TIM5_SMCR ( read-write )  \ slave mode control register
    TIM5 $C + constant TIM5_DIER ( read-write )  \ DMA/Interrupt enable register
    TIM5 $10 + constant TIM5_SR ( read-write )  \ status register
    TIM5 $14 + constant TIM5_EGR ( write-only )  \ event generation register
    TIM5 $18 + constant TIM5_CCMR1_Output ( read-write )  \ capture/compare mode register 1 output mode
    TIM5 $18 + constant TIM5_CCMR1_Input ( read-write )  \ capture/compare mode register 1 input mode
    TIM5 $1C + constant TIM5_CCMR2_Output ( read-write )  \ capture/compare mode register 2 output mode
    TIM5 $1C + constant TIM5_CCMR2_Input ( read-write )  \ capture/compare mode register 2 input mode
    TIM5 $20 + constant TIM5_CCER ( read-write )  \ capture/compare enable register
    TIM5 $24 + constant TIM5_CNT ( read-write )  \ counter
    TIM5 $28 + constant TIM5_PSC ( read-write )  \ prescaler
    TIM5 $2C + constant TIM5_ARR ( read-write )  \ auto-reload register
    TIM5 $34 + constant TIM5_CCR1 ( read-write )  \ capture/compare register 1
    TIM5 $38 + constant TIM5_CCR2 ( read-write )  \ capture/compare register 2
    TIM5 $3C + constant TIM5_CCR3 ( read-write )  \ capture/compare register 3
    TIM5 $40 + constant TIM5_CCR4 ( read-write )  \ capture/compare register 4
    TIM5 $48 + constant TIM5_DCR ( read-write )  \ DMA control register
    TIM5 $4C + constant TIM5_DMAR ( read-write )  \ DMA address for full transfer
    : TIM5_CR1. cr ." TIM5_CR1.  RW   $" TIM5_CR1 @ hex. TIM5_CR1 1b. ;
    : TIM5_CR2. cr ." TIM5_CR2.  RW   $" TIM5_CR2 @ hex. TIM5_CR2 1b. ;
    : TIM5_SMCR. cr ." TIM5_SMCR.  RW   $" TIM5_SMCR @ hex. TIM5_SMCR 1b. ;
    : TIM5_DIER. cr ." TIM5_DIER.  RW   $" TIM5_DIER @ hex. TIM5_DIER 1b. ;
    : TIM5_SR. cr ." TIM5_SR.  RW   $" TIM5_SR @ hex. TIM5_SR 1b. ;
    : TIM5_EGR. cr ." TIM5_EGR " WRITEONLY ; 
    : TIM5_CCMR1_Output. cr ." TIM5_CCMR1_Output.  RW   $" TIM5_CCMR1_Output @ hex. TIM5_CCMR1_Output 1b. ;
    : TIM5_CCMR1_Input. cr ." TIM5_CCMR1_Input.  RW   $" TIM5_CCMR1_Input @ hex. TIM5_CCMR1_Input 1b. ;
    : TIM5_CCMR2_Output. cr ." TIM5_CCMR2_Output.  RW   $" TIM5_CCMR2_Output @ hex. TIM5_CCMR2_Output 1b. ;
    : TIM5_CCMR2_Input. cr ." TIM5_CCMR2_Input.  RW   $" TIM5_CCMR2_Input @ hex. TIM5_CCMR2_Input 1b. ;
    : TIM5_CCER. cr ." TIM5_CCER.  RW   $" TIM5_CCER @ hex. TIM5_CCER 1b. ;
    : TIM5_CNT. cr ." TIM5_CNT.  RW   $" TIM5_CNT @ hex. TIM5_CNT 1b. ;
    : TIM5_PSC. cr ." TIM5_PSC.  RW   $" TIM5_PSC @ hex. TIM5_PSC 1b. ;
    : TIM5_ARR. cr ." TIM5_ARR.  RW   $" TIM5_ARR @ hex. TIM5_ARR 1b. ;
    : TIM5_CCR1. cr ." TIM5_CCR1.  RW   $" TIM5_CCR1 @ hex. TIM5_CCR1 1b. ;
    : TIM5_CCR2. cr ." TIM5_CCR2.  RW   $" TIM5_CCR2 @ hex. TIM5_CCR2 1b. ;
    : TIM5_CCR3. cr ." TIM5_CCR3.  RW   $" TIM5_CCR3 @ hex. TIM5_CCR3 1b. ;
    : TIM5_CCR4. cr ." TIM5_CCR4.  RW   $" TIM5_CCR4 @ hex. TIM5_CCR4 1b. ;
    : TIM5_DCR. cr ." TIM5_DCR.  RW   $" TIM5_DCR @ hex. TIM5_DCR 1b. ;
    : TIM5_DMAR. cr ." TIM5_DMAR.  RW   $" TIM5_DMAR @ hex. TIM5_DMAR 1b. ;
    : TIM5.
      TIM5_CR1.
      TIM5_CR2.
      TIM5_SMCR.
      TIM5_DIER.
      TIM5_SR.
      TIM5_EGR.
      TIM5_CCMR1_Output.
      TIM5_CCMR1_Input.
      TIM5_CCMR2_Output.
      TIM5_CCMR2_Input.
      TIM5_CCER.
      TIM5_CNT.
      TIM5_PSC.
      TIM5_ARR.
      TIM5_CCR1.
      TIM5_CCR2.
      TIM5_CCR3.
      TIM5_CCR4.
      TIM5_DCR.
      TIM5_DMAR.
    ;
  [then]

  execute-defined? use-TIM9 [if]
    $40014000 constant TIM9 ( General purpose timers ) 
    TIM9 $0 + constant TIM9_CR1 ( read-write )  \ control register 1
    TIM9 $8 + constant TIM9_SMCR ( read-write )  \ slave mode control register
    TIM9 $C + constant TIM9_DIER ( read-write )  \ DMA/Interrupt enable register
    TIM9 $10 + constant TIM9_SR ( read-write )  \ status register
    TIM9 $14 + constant TIM9_EGR ( write-only )  \ event generation register
    TIM9 $18 + constant TIM9_CCMR1_Output ( read-write )  \ capture/compare mode register 1 output mode
    TIM9 $18 + constant TIM9_CCMR1_Input ( read-write )  \ capture/compare mode register 1 input mode
    TIM9 $20 + constant TIM9_CCER ( read-write )  \ capture/compare enable register
    TIM9 $24 + constant TIM9_CNT ( read-write )  \ counter
    TIM9 $28 + constant TIM9_PSC ( read-write )  \ prescaler
    TIM9 $2C + constant TIM9_ARR ( read-write )  \ auto-reload register
    TIM9 $34 + constant TIM9_CCR1 ( read-write )  \ capture/compare register 1
    TIM9 $38 + constant TIM9_CCR2 ( read-write )  \ capture/compare register 2
    : TIM9_CR1. cr ." TIM9_CR1.  RW   $" TIM9_CR1 @ hex. TIM9_CR1 1b. ;
    : TIM9_SMCR. cr ." TIM9_SMCR.  RW   $" TIM9_SMCR @ hex. TIM9_SMCR 1b. ;
    : TIM9_DIER. cr ." TIM9_DIER.  RW   $" TIM9_DIER @ hex. TIM9_DIER 1b. ;
    : TIM9_SR. cr ." TIM9_SR.  RW   $" TIM9_SR @ hex. TIM9_SR 1b. ;
    : TIM9_EGR. cr ." TIM9_EGR " WRITEONLY ; 
    : TIM9_CCMR1_Output. cr ." TIM9_CCMR1_Output.  RW   $" TIM9_CCMR1_Output @ hex. TIM9_CCMR1_Output 1b. ;
    : TIM9_CCMR1_Input. cr ." TIM9_CCMR1_Input.  RW   $" TIM9_CCMR1_Input @ hex. TIM9_CCMR1_Input 1b. ;
    : TIM9_CCER. cr ." TIM9_CCER.  RW   $" TIM9_CCER @ hex. TIM9_CCER 1b. ;
    : TIM9_CNT. cr ." TIM9_CNT.  RW   $" TIM9_CNT @ hex. TIM9_CNT 1b. ;
    : TIM9_PSC. cr ." TIM9_PSC.  RW   $" TIM9_PSC @ hex. TIM9_PSC 1b. ;
    : TIM9_ARR. cr ." TIM9_ARR.  RW   $" TIM9_ARR @ hex. TIM9_ARR 1b. ;
    : TIM9_CCR1. cr ." TIM9_CCR1.  RW   $" TIM9_CCR1 @ hex. TIM9_CCR1 1b. ;
    : TIM9_CCR2. cr ." TIM9_CCR2.  RW   $" TIM9_CCR2 @ hex. TIM9_CCR2 1b. ;
    : TIM9.
      TIM9_CR1.
      TIM9_SMCR.
      TIM9_DIER.
      TIM9_SR.
      TIM9_EGR.
      TIM9_CCMR1_Output.
      TIM9_CCMR1_Input.
      TIM9_CCER.
      TIM9_CNT.
      TIM9_PSC.
      TIM9_ARR.
      TIM9_CCR1.
      TIM9_CCR2.
    ;
  [then]

  execute-defined? use-TIM12 [if]
    $40001800 constant TIM12 ( General purpose timers ) 
    TIM12 $0 + constant TIM12_CR1 ( read-write )  \ control register 1
    TIM12 $8 + constant TIM12_SMCR ( read-write )  \ slave mode control register
    TIM12 $C + constant TIM12_DIER ( read-write )  \ DMA/Interrupt enable register
    TIM12 $10 + constant TIM12_SR ( read-write )  \ status register
    TIM12 $14 + constant TIM12_EGR ( write-only )  \ event generation register
    TIM12 $18 + constant TIM12_CCMR1_Output ( read-write )  \ capture/compare mode register 1 output mode
    TIM12 $18 + constant TIM12_CCMR1_Input ( read-write )  \ capture/compare mode register 1 input mode
    TIM12 $20 + constant TIM12_CCER ( read-write )  \ capture/compare enable register
    TIM12 $24 + constant TIM12_CNT ( read-write )  \ counter
    TIM12 $28 + constant TIM12_PSC ( read-write )  \ prescaler
    TIM12 $2C + constant TIM12_ARR ( read-write )  \ auto-reload register
    TIM12 $34 + constant TIM12_CCR1 ( read-write )  \ capture/compare register 1
    TIM12 $38 + constant TIM12_CCR2 ( read-write )  \ capture/compare register 2
    : TIM12_CR1. cr ." TIM12_CR1.  RW   $" TIM12_CR1 @ hex. TIM12_CR1 1b. ;
    : TIM12_SMCR. cr ." TIM12_SMCR.  RW   $" TIM12_SMCR @ hex. TIM12_SMCR 1b. ;
    : TIM12_DIER. cr ." TIM12_DIER.  RW   $" TIM12_DIER @ hex. TIM12_DIER 1b. ;
    : TIM12_SR. cr ." TIM12_SR.  RW   $" TIM12_SR @ hex. TIM12_SR 1b. ;
    : TIM12_EGR. cr ." TIM12_EGR " WRITEONLY ; 
    : TIM12_CCMR1_Output. cr ." TIM12_CCMR1_Output.  RW   $" TIM12_CCMR1_Output @ hex. TIM12_CCMR1_Output 1b. ;
    : TIM12_CCMR1_Input. cr ." TIM12_CCMR1_Input.  RW   $" TIM12_CCMR1_Input @ hex. TIM12_CCMR1_Input 1b. ;
    : TIM12_CCER. cr ." TIM12_CCER.  RW   $" TIM12_CCER @ hex. TIM12_CCER 1b. ;
    : TIM12_CNT. cr ." TIM12_CNT.  RW   $" TIM12_CNT @ hex. TIM12_CNT 1b. ;
    : TIM12_PSC. cr ." TIM12_PSC.  RW   $" TIM12_PSC @ hex. TIM12_PSC 1b. ;
    : TIM12_ARR. cr ." TIM12_ARR.  RW   $" TIM12_ARR @ hex. TIM12_ARR 1b. ;
    : TIM12_CCR1. cr ." TIM12_CCR1.  RW   $" TIM12_CCR1 @ hex. TIM12_CCR1 1b. ;
    : TIM12_CCR2. cr ." TIM12_CCR2.  RW   $" TIM12_CCR2 @ hex. TIM12_CCR2 1b. ;
    : TIM12.
      TIM12_CR1.
      TIM12_SMCR.
      TIM12_DIER.
      TIM12_SR.
      TIM12_EGR.
      TIM12_CCMR1_Output.
      TIM12_CCMR1_Input.
      TIM12_CCER.
      TIM12_CNT.
      TIM12_PSC.
      TIM12_ARR.
      TIM12_CCR1.
      TIM12_CCR2.
    ;
  [then]

  execute-defined? use-TIM10 [if]
    $40014400 constant TIM10 ( General-purpose-timers ) 
    TIM10 $0 + constant TIM10_CR1 ( read-write )  \ control register 1
    TIM10 $C + constant TIM10_DIER ( read-write )  \ DMA/Interrupt enable register
    TIM10 $10 + constant TIM10_SR ( read-write )  \ status register
    TIM10 $14 + constant TIM10_EGR ( write-only )  \ event generation register
    TIM10 $18 + constant TIM10_CCMR1_Output ( read-write )  \ capture/compare mode register 1 output mode
    TIM10 $18 + constant TIM10_CCMR1_Input ( read-write )  \ capture/compare mode register 1 input mode
    TIM10 $20 + constant TIM10_CCER ( read-write )  \ capture/compare enable register
    TIM10 $24 + constant TIM10_CNT ( read-write )  \ counter
    TIM10 $28 + constant TIM10_PSC ( read-write )  \ prescaler
    TIM10 $2C + constant TIM10_ARR ( read-write )  \ auto-reload register
    TIM10 $34 + constant TIM10_CCR1 ( read-write )  \ capture/compare register 1
    TIM10 $8 + constant TIM10_SMCR ( read-write )  \ slave mode control register
    TIM10 $50 + constant TIM10_OR ( read-write )  \ option register
    : TIM10_CR1. cr ." TIM10_CR1.  RW   $" TIM10_CR1 @ hex. TIM10_CR1 1b. ;
    : TIM10_DIER. cr ." TIM10_DIER.  RW   $" TIM10_DIER @ hex. TIM10_DIER 1b. ;
    : TIM10_SR. cr ." TIM10_SR.  RW   $" TIM10_SR @ hex. TIM10_SR 1b. ;
    : TIM10_EGR. cr ." TIM10_EGR " WRITEONLY ; 
    : TIM10_CCMR1_Output. cr ." TIM10_CCMR1_Output.  RW   $" TIM10_CCMR1_Output @ hex. TIM10_CCMR1_Output 1b. ;
    : TIM10_CCMR1_Input. cr ." TIM10_CCMR1_Input.  RW   $" TIM10_CCMR1_Input @ hex. TIM10_CCMR1_Input 1b. ;
    : TIM10_CCER. cr ." TIM10_CCER.  RW   $" TIM10_CCER @ hex. TIM10_CCER 1b. ;
    : TIM10_CNT. cr ." TIM10_CNT.  RW   $" TIM10_CNT @ hex. TIM10_CNT 1b. ;
    : TIM10_PSC. cr ." TIM10_PSC.  RW   $" TIM10_PSC @ hex. TIM10_PSC 1b. ;
    : TIM10_ARR. cr ." TIM10_ARR.  RW   $" TIM10_ARR @ hex. TIM10_ARR 1b. ;
    : TIM10_CCR1. cr ." TIM10_CCR1.  RW   $" TIM10_CCR1 @ hex. TIM10_CCR1 1b. ;
    : TIM10_SMCR. cr ." TIM10_SMCR.  RW   $" TIM10_SMCR @ hex. TIM10_SMCR 1b. ;
    : TIM10_OR. cr ." TIM10_OR.  RW   $" TIM10_OR @ hex. TIM10_OR 1b. ;
    : TIM10.
      TIM10_CR1.
      TIM10_DIER.
      TIM10_SR.
      TIM10_EGR.
      TIM10_CCMR1_Output.
      TIM10_CCMR1_Input.
      TIM10_CCER.
      TIM10_CNT.
      TIM10_PSC.
      TIM10_ARR.
      TIM10_CCR1.
      TIM10_SMCR.
      TIM10_OR.
    ;
  [then]

  execute-defined? use-TIM11 [if]
    $40014800 constant TIM11 ( General-purpose-timers ) 
    TIM11 $0 + constant TIM11_CR1 ( read-write )  \ control register 1
    TIM11 $C + constant TIM11_DIER ( read-write )  \ DMA/Interrupt enable register
    TIM11 $10 + constant TIM11_SR ( read-write )  \ status register
    TIM11 $14 + constant TIM11_EGR ( write-only )  \ event generation register
    TIM11 $18 + constant TIM11_CCMR1_Output ( read-write )  \ capture/compare mode register 1 output mode
    TIM11 $18 + constant TIM11_CCMR1_Input ( read-write )  \ capture/compare mode register 1 input mode
    TIM11 $20 + constant TIM11_CCER ( read-write )  \ capture/compare enable register
    TIM11 $24 + constant TIM11_CNT ( read-write )  \ counter
    TIM11 $28 + constant TIM11_PSC ( read-write )  \ prescaler
    TIM11 $2C + constant TIM11_ARR ( read-write )  \ auto-reload register
    TIM11 $34 + constant TIM11_CCR1 ( read-write )  \ capture/compare register 1
    TIM11 $8 + constant TIM11_SMCR ( read-write )  \ slave mode control register
    TIM11 $50 + constant TIM11_OR ( read-write )  \ option register
    : TIM11_CR1. cr ." TIM11_CR1.  RW   $" TIM11_CR1 @ hex. TIM11_CR1 1b. ;
    : TIM11_DIER. cr ." TIM11_DIER.  RW   $" TIM11_DIER @ hex. TIM11_DIER 1b. ;
    : TIM11_SR. cr ." TIM11_SR.  RW   $" TIM11_SR @ hex. TIM11_SR 1b. ;
    : TIM11_EGR. cr ." TIM11_EGR " WRITEONLY ; 
    : TIM11_CCMR1_Output. cr ." TIM11_CCMR1_Output.  RW   $" TIM11_CCMR1_Output @ hex. TIM11_CCMR1_Output 1b. ;
    : TIM11_CCMR1_Input. cr ." TIM11_CCMR1_Input.  RW   $" TIM11_CCMR1_Input @ hex. TIM11_CCMR1_Input 1b. ;
    : TIM11_CCER. cr ." TIM11_CCER.  RW   $" TIM11_CCER @ hex. TIM11_CCER 1b. ;
    : TIM11_CNT. cr ." TIM11_CNT.  RW   $" TIM11_CNT @ hex. TIM11_CNT 1b. ;
    : TIM11_PSC. cr ." TIM11_PSC.  RW   $" TIM11_PSC @ hex. TIM11_PSC 1b. ;
    : TIM11_ARR. cr ." TIM11_ARR.  RW   $" TIM11_ARR @ hex. TIM11_ARR 1b. ;
    : TIM11_CCR1. cr ." TIM11_CCR1.  RW   $" TIM11_CCR1 @ hex. TIM11_CCR1 1b. ;
    : TIM11_SMCR. cr ." TIM11_SMCR.  RW   $" TIM11_SMCR @ hex. TIM11_SMCR 1b. ;
    : TIM11_OR. cr ." TIM11_OR.  RW   $" TIM11_OR @ hex. TIM11_OR 1b. ;
    : TIM11.
      TIM11_CR1.
      TIM11_DIER.
      TIM11_SR.
      TIM11_EGR.
      TIM11_CCMR1_Output.
      TIM11_CCMR1_Input.
      TIM11_CCER.
      TIM11_CNT.
      TIM11_PSC.
      TIM11_ARR.
      TIM11_CCR1.
      TIM11_SMCR.
      TIM11_OR.
    ;
  [then]

  execute-defined? use-TIM13 [if]
    $40001C00 constant TIM13 ( General-purpose-timers ) 
    TIM13 $0 + constant TIM13_CR1 ( read-write )  \ control register 1
    TIM13 $C + constant TIM13_DIER ( read-write )  \ DMA/Interrupt enable register
    TIM13 $10 + constant TIM13_SR ( read-write )  \ status register
    TIM13 $14 + constant TIM13_EGR ( write-only )  \ event generation register
    TIM13 $18 + constant TIM13_CCMR1_Output ( read-write )  \ capture/compare mode register 1 output mode
    TIM13 $18 + constant TIM13_CCMR1_Input ( read-write )  \ capture/compare mode register 1 input mode
    TIM13 $20 + constant TIM13_CCER ( read-write )  \ capture/compare enable register
    TIM13 $24 + constant TIM13_CNT ( read-write )  \ counter
    TIM13 $28 + constant TIM13_PSC ( read-write )  \ prescaler
    TIM13 $2C + constant TIM13_ARR ( read-write )  \ auto-reload register
    TIM13 $34 + constant TIM13_CCR1 ( read-write )  \ capture/compare register 1
    TIM13 $8 + constant TIM13_SMCR ( read-write )  \ slave mode control register
    TIM13 $50 + constant TIM13_OR ( read-write )  \ option register
    : TIM13_CR1. cr ." TIM13_CR1.  RW   $" TIM13_CR1 @ hex. TIM13_CR1 1b. ;
    : TIM13_DIER. cr ." TIM13_DIER.  RW   $" TIM13_DIER @ hex. TIM13_DIER 1b. ;
    : TIM13_SR. cr ." TIM13_SR.  RW   $" TIM13_SR @ hex. TIM13_SR 1b. ;
    : TIM13_EGR. cr ." TIM13_EGR " WRITEONLY ; 
    : TIM13_CCMR1_Output. cr ." TIM13_CCMR1_Output.  RW   $" TIM13_CCMR1_Output @ hex. TIM13_CCMR1_Output 1b. ;
    : TIM13_CCMR1_Input. cr ." TIM13_CCMR1_Input.  RW   $" TIM13_CCMR1_Input @ hex. TIM13_CCMR1_Input 1b. ;
    : TIM13_CCER. cr ." TIM13_CCER.  RW   $" TIM13_CCER @ hex. TIM13_CCER 1b. ;
    : TIM13_CNT. cr ." TIM13_CNT.  RW   $" TIM13_CNT @ hex. TIM13_CNT 1b. ;
    : TIM13_PSC. cr ." TIM13_PSC.  RW   $" TIM13_PSC @ hex. TIM13_PSC 1b. ;
    : TIM13_ARR. cr ." TIM13_ARR.  RW   $" TIM13_ARR @ hex. TIM13_ARR 1b. ;
    : TIM13_CCR1. cr ." TIM13_CCR1.  RW   $" TIM13_CCR1 @ hex. TIM13_CCR1 1b. ;
    : TIM13_SMCR. cr ." TIM13_SMCR.  RW   $" TIM13_SMCR @ hex. TIM13_SMCR 1b. ;
    : TIM13_OR. cr ." TIM13_OR.  RW   $" TIM13_OR @ hex. TIM13_OR 1b. ;
    : TIM13.
      TIM13_CR1.
      TIM13_DIER.
      TIM13_SR.
      TIM13_EGR.
      TIM13_CCMR1_Output.
      TIM13_CCMR1_Input.
      TIM13_CCER.
      TIM13_CNT.
      TIM13_PSC.
      TIM13_ARR.
      TIM13_CCR1.
      TIM13_SMCR.
      TIM13_OR.
    ;
  [then]

  execute-defined? use-TIM14 [if]
    $40002000 constant TIM14 ( General-purpose-timers ) 
    TIM14 $0 + constant TIM14_CR1 ( read-write )  \ control register 1
    TIM14 $C + constant TIM14_DIER ( read-write )  \ DMA/Interrupt enable register
    TIM14 $10 + constant TIM14_SR ( read-write )  \ status register
    TIM14 $14 + constant TIM14_EGR ( write-only )  \ event generation register
    TIM14 $18 + constant TIM14_CCMR1_Output ( read-write )  \ capture/compare mode register 1 output mode
    TIM14 $18 + constant TIM14_CCMR1_Input ( read-write )  \ capture/compare mode register 1 input mode
    TIM14 $20 + constant TIM14_CCER ( read-write )  \ capture/compare enable register
    TIM14 $24 + constant TIM14_CNT ( read-write )  \ counter
    TIM14 $28 + constant TIM14_PSC ( read-write )  \ prescaler
    TIM14 $2C + constant TIM14_ARR ( read-write )  \ auto-reload register
    TIM14 $34 + constant TIM14_CCR1 ( read-write )  \ capture/compare register 1
    TIM14 $8 + constant TIM14_SMCR ( read-write )  \ slave mode control register
    TIM14 $50 + constant TIM14_OR ( read-write )  \ option register
    : TIM14_CR1. cr ." TIM14_CR1.  RW   $" TIM14_CR1 @ hex. TIM14_CR1 1b. ;
    : TIM14_DIER. cr ." TIM14_DIER.  RW   $" TIM14_DIER @ hex. TIM14_DIER 1b. ;
    : TIM14_SR. cr ." TIM14_SR.  RW   $" TIM14_SR @ hex. TIM14_SR 1b. ;
    : TIM14_EGR. cr ." TIM14_EGR " WRITEONLY ; 
    : TIM14_CCMR1_Output. cr ." TIM14_CCMR1_Output.  RW   $" TIM14_CCMR1_Output @ hex. TIM14_CCMR1_Output 1b. ;
    : TIM14_CCMR1_Input. cr ." TIM14_CCMR1_Input.  RW   $" TIM14_CCMR1_Input @ hex. TIM14_CCMR1_Input 1b. ;
    : TIM14_CCER. cr ." TIM14_CCER.  RW   $" TIM14_CCER @ hex. TIM14_CCER 1b. ;
    : TIM14_CNT. cr ." TIM14_CNT.  RW   $" TIM14_CNT @ hex. TIM14_CNT 1b. ;
    : TIM14_PSC. cr ." TIM14_PSC.  RW   $" TIM14_PSC @ hex. TIM14_PSC 1b. ;
    : TIM14_ARR. cr ." TIM14_ARR.  RW   $" TIM14_ARR @ hex. TIM14_ARR 1b. ;
    : TIM14_CCR1. cr ." TIM14_CCR1.  RW   $" TIM14_CCR1 @ hex. TIM14_CCR1 1b. ;
    : TIM14_SMCR. cr ." TIM14_SMCR.  RW   $" TIM14_SMCR @ hex. TIM14_SMCR 1b. ;
    : TIM14_OR. cr ." TIM14_OR.  RW   $" TIM14_OR @ hex. TIM14_OR 1b. ;
    : TIM14.
      TIM14_CR1.
      TIM14_DIER.
      TIM14_SR.
      TIM14_EGR.
      TIM14_CCMR1_Output.
      TIM14_CCMR1_Input.
      TIM14_CCER.
      TIM14_CNT.
      TIM14_PSC.
      TIM14_ARR.
      TIM14_CCR1.
      TIM14_SMCR.
      TIM14_OR.
    ;
  [then]

  execute-defined? use-TIM6 [if]
    $40001000 constant TIM6 ( Basic timers ) 
    TIM6 $0 + constant TIM6_CR1 ( read-write )  \ control register 1
    TIM6 $4 + constant TIM6_CR2 ( read-write )  \ control register 2
    TIM6 $C + constant TIM6_DIER ( read-write )  \ DMA/Interrupt enable register
    TIM6 $10 + constant TIM6_SR ( read-write )  \ status register
    TIM6 $14 + constant TIM6_EGR ( write-only )  \ event generation register
    TIM6 $24 + constant TIM6_CNT ( read-write )  \ counter
    TIM6 $28 + constant TIM6_PSC ( read-write )  \ prescaler
    TIM6 $2C + constant TIM6_ARR ( read-write )  \ auto-reload register
    : TIM6_CR1. cr ." TIM6_CR1.  RW   $" TIM6_CR1 @ hex. TIM6_CR1 1b. ;
    : TIM6_CR2. cr ." TIM6_CR2.  RW   $" TIM6_CR2 @ hex. TIM6_CR2 1b. ;
    : TIM6_DIER. cr ." TIM6_DIER.  RW   $" TIM6_DIER @ hex. TIM6_DIER 1b. ;
    : TIM6_SR. cr ." TIM6_SR.  RW   $" TIM6_SR @ hex. TIM6_SR 1b. ;
    : TIM6_EGR. cr ." TIM6_EGR " WRITEONLY ; 
    : TIM6_CNT. cr ." TIM6_CNT.  RW   $" TIM6_CNT @ hex. TIM6_CNT 1b. ;
    : TIM6_PSC. cr ." TIM6_PSC.  RW   $" TIM6_PSC @ hex. TIM6_PSC 1b. ;
    : TIM6_ARR. cr ." TIM6_ARR.  RW   $" TIM6_ARR @ hex. TIM6_ARR 1b. ;
    : TIM6.
      TIM6_CR1.
      TIM6_CR2.
      TIM6_DIER.
      TIM6_SR.
      TIM6_EGR.
      TIM6_CNT.
      TIM6_PSC.
      TIM6_ARR.
    ;
  [then]

  execute-defined? use-TIM7 [if]
    $40001400 constant TIM7 ( Basic timers ) 
    TIM7 $0 + constant TIM7_CR1 ( read-write )  \ control register 1
    TIM7 $4 + constant TIM7_CR2 ( read-write )  \ control register 2
    TIM7 $C + constant TIM7_DIER ( read-write )  \ DMA/Interrupt enable register
    TIM7 $10 + constant TIM7_SR ( read-write )  \ status register
    TIM7 $14 + constant TIM7_EGR ( write-only )  \ event generation register
    TIM7 $24 + constant TIM7_CNT ( read-write )  \ counter
    TIM7 $28 + constant TIM7_PSC ( read-write )  \ prescaler
    TIM7 $2C + constant TIM7_ARR ( read-write )  \ auto-reload register
    : TIM7_CR1. cr ." TIM7_CR1.  RW   $" TIM7_CR1 @ hex. TIM7_CR1 1b. ;
    : TIM7_CR2. cr ." TIM7_CR2.  RW   $" TIM7_CR2 @ hex. TIM7_CR2 1b. ;
    : TIM7_DIER. cr ." TIM7_DIER.  RW   $" TIM7_DIER @ hex. TIM7_DIER 1b. ;
    : TIM7_SR. cr ." TIM7_SR.  RW   $" TIM7_SR @ hex. TIM7_SR 1b. ;
    : TIM7_EGR. cr ." TIM7_EGR " WRITEONLY ; 
    : TIM7_CNT. cr ." TIM7_CNT.  RW   $" TIM7_CNT @ hex. TIM7_CNT 1b. ;
    : TIM7_PSC. cr ." TIM7_PSC.  RW   $" TIM7_PSC @ hex. TIM7_PSC 1b. ;
    : TIM7_ARR. cr ." TIM7_ARR.  RW   $" TIM7_ARR @ hex. TIM7_ARR 1b. ;
    : TIM7.
      TIM7_CR1.
      TIM7_CR2.
      TIM7_DIER.
      TIM7_SR.
      TIM7_EGR.
      TIM7_CNT.
      TIM7_PSC.
      TIM7_ARR.
    ;
  [then]

  execute-defined? use-Ethernet_MAC [if]
    $40028000 constant Ethernet_MAC ( Ethernet: media access control MAC ) 
    Ethernet_MAC $0 + constant Ethernet_MAC_MACCR ( read-write )  \ Ethernet MAC configuration register
    Ethernet_MAC $4 + constant Ethernet_MAC_MACFFR ( read-write )  \ Ethernet MAC frame filter register
    Ethernet_MAC $8 + constant Ethernet_MAC_MACHTHR ( read-write )  \ Ethernet MAC hash table high register
    Ethernet_MAC $C + constant Ethernet_MAC_MACHTLR ( read-write )  \ Ethernet MAC hash table low register
    Ethernet_MAC $10 + constant Ethernet_MAC_MACMIIAR ( read-write )  \ Ethernet MAC MII address register
    Ethernet_MAC $14 + constant Ethernet_MAC_MACMIIDR ( read-write )  \ Ethernet MAC MII data register
    Ethernet_MAC $18 + constant Ethernet_MAC_MACFCR ( read-write )  \ Ethernet MAC flow control register
    Ethernet_MAC $1C + constant Ethernet_MAC_MACVLANTR ( read-write )  \ Ethernet MAC VLAN tag register
    Ethernet_MAC $2C + constant Ethernet_MAC_MACPMTCSR ( read-write )  \ Ethernet MAC PMT control and status register
    Ethernet_MAC $34 + constant Ethernet_MAC_MACDBGR ( read-only )  \ Ethernet MAC debug register
    Ethernet_MAC $38 + constant Ethernet_MAC_MACSR (  )  \ Ethernet MAC interrupt status register
    Ethernet_MAC $3C + constant Ethernet_MAC_MACIMR ( read-write )  \ Ethernet MAC interrupt mask register
    Ethernet_MAC $40 + constant Ethernet_MAC_MACA0HR (  )  \ Ethernet MAC address 0 high register
    Ethernet_MAC $44 + constant Ethernet_MAC_MACA0LR ( read-write )  \ Ethernet MAC address 0 low register
    Ethernet_MAC $48 + constant Ethernet_MAC_MACA1HR ( read-write )  \ Ethernet MAC address 1 high register
    Ethernet_MAC $4C + constant Ethernet_MAC_MACA1LR ( read-write )  \ Ethernet MAC address1 low register
    Ethernet_MAC $50 + constant Ethernet_MAC_MACA2HR ( read-write )  \ Ethernet MAC address 2 high register
    Ethernet_MAC $54 + constant Ethernet_MAC_MACA2LR ( read-write )  \ Ethernet MAC address 2 low register
    Ethernet_MAC $58 + constant Ethernet_MAC_MACA3HR ( read-write )  \ Ethernet MAC address 3 high register
    Ethernet_MAC $5C + constant Ethernet_MAC_MACA3LR ( read-write )  \ Ethernet MAC address 3 low register
    Ethernet_MAC $60 + constant Ethernet_MAC_MACRWUFFER ( read-write )  \ Ethernet MAC remote wakeup frame filter register
    : Ethernet_MAC_MACCR. cr ." Ethernet_MAC_MACCR.  RW   $" Ethernet_MAC_MACCR @ hex. Ethernet_MAC_MACCR 1b. ;
    : Ethernet_MAC_MACFFR. cr ." Ethernet_MAC_MACFFR.  RW   $" Ethernet_MAC_MACFFR @ hex. Ethernet_MAC_MACFFR 1b. ;
    : Ethernet_MAC_MACHTHR. cr ." Ethernet_MAC_MACHTHR.  RW   $" Ethernet_MAC_MACHTHR @ hex. Ethernet_MAC_MACHTHR 1b. ;
    : Ethernet_MAC_MACHTLR. cr ." Ethernet_MAC_MACHTLR.  RW   $" Ethernet_MAC_MACHTLR @ hex. Ethernet_MAC_MACHTLR 1b. ;
    : Ethernet_MAC_MACMIIAR. cr ." Ethernet_MAC_MACMIIAR.  RW   $" Ethernet_MAC_MACMIIAR @ hex. Ethernet_MAC_MACMIIAR 1b. ;
    : Ethernet_MAC_MACMIIDR. cr ." Ethernet_MAC_MACMIIDR.  RW   $" Ethernet_MAC_MACMIIDR @ hex. Ethernet_MAC_MACMIIDR 1b. ;
    : Ethernet_MAC_MACFCR. cr ." Ethernet_MAC_MACFCR.  RW   $" Ethernet_MAC_MACFCR @ hex. Ethernet_MAC_MACFCR 1b. ;
    : Ethernet_MAC_MACVLANTR. cr ." Ethernet_MAC_MACVLANTR.  RW   $" Ethernet_MAC_MACVLANTR @ hex. Ethernet_MAC_MACVLANTR 1b. ;
    : Ethernet_MAC_MACPMTCSR. cr ." Ethernet_MAC_MACPMTCSR.  RW   $" Ethernet_MAC_MACPMTCSR @ hex. Ethernet_MAC_MACPMTCSR 1b. ;
    : Ethernet_MAC_MACDBGR. cr ." Ethernet_MAC_MACDBGR.  RO   $" Ethernet_MAC_MACDBGR @ hex. Ethernet_MAC_MACDBGR 1b. ;
    : Ethernet_MAC_MACSR. cr ." Ethernet_MAC_MACSR.   $" Ethernet_MAC_MACSR @ hex. Ethernet_MAC_MACSR 1b. ;
    : Ethernet_MAC_MACIMR. cr ." Ethernet_MAC_MACIMR.  RW   $" Ethernet_MAC_MACIMR @ hex. Ethernet_MAC_MACIMR 1b. ;
    : Ethernet_MAC_MACA0HR. cr ." Ethernet_MAC_MACA0HR.   $" Ethernet_MAC_MACA0HR @ hex. Ethernet_MAC_MACA0HR 1b. ;
    : Ethernet_MAC_MACA0LR. cr ." Ethernet_MAC_MACA0LR.  RW   $" Ethernet_MAC_MACA0LR @ hex. Ethernet_MAC_MACA0LR 1b. ;
    : Ethernet_MAC_MACA1HR. cr ." Ethernet_MAC_MACA1HR.  RW   $" Ethernet_MAC_MACA1HR @ hex. Ethernet_MAC_MACA1HR 1b. ;
    : Ethernet_MAC_MACA1LR. cr ." Ethernet_MAC_MACA1LR.  RW   $" Ethernet_MAC_MACA1LR @ hex. Ethernet_MAC_MACA1LR 1b. ;
    : Ethernet_MAC_MACA2HR. cr ." Ethernet_MAC_MACA2HR.  RW   $" Ethernet_MAC_MACA2HR @ hex. Ethernet_MAC_MACA2HR 1b. ;
    : Ethernet_MAC_MACA2LR. cr ." Ethernet_MAC_MACA2LR.  RW   $" Ethernet_MAC_MACA2LR @ hex. Ethernet_MAC_MACA2LR 1b. ;
    : Ethernet_MAC_MACA3HR. cr ." Ethernet_MAC_MACA3HR.  RW   $" Ethernet_MAC_MACA3HR @ hex. Ethernet_MAC_MACA3HR 1b. ;
    : Ethernet_MAC_MACA3LR. cr ." Ethernet_MAC_MACA3LR.  RW   $" Ethernet_MAC_MACA3LR @ hex. Ethernet_MAC_MACA3LR 1b. ;
    : Ethernet_MAC_MACRWUFFER. cr ." Ethernet_MAC_MACRWUFFER.  RW   $" Ethernet_MAC_MACRWUFFER @ hex. Ethernet_MAC_MACRWUFFER 1b. ;
    : Ethernet_MAC.
      Ethernet_MAC_MACCR.
      Ethernet_MAC_MACFFR.
      Ethernet_MAC_MACHTHR.
      Ethernet_MAC_MACHTLR.
      Ethernet_MAC_MACMIIAR.
      Ethernet_MAC_MACMIIDR.
      Ethernet_MAC_MACFCR.
      Ethernet_MAC_MACVLANTR.
      Ethernet_MAC_MACPMTCSR.
      Ethernet_MAC_MACDBGR.
      Ethernet_MAC_MACSR.
      Ethernet_MAC_MACIMR.
      Ethernet_MAC_MACA0HR.
      Ethernet_MAC_MACA0LR.
      Ethernet_MAC_MACA1HR.
      Ethernet_MAC_MACA1LR.
      Ethernet_MAC_MACA2HR.
      Ethernet_MAC_MACA2LR.
      Ethernet_MAC_MACA3HR.
      Ethernet_MAC_MACA3LR.
      Ethernet_MAC_MACRWUFFER.
    ;
  [then]

  execute-defined? use-Ethernet_MMC [if]
    $40028100 constant Ethernet_MMC ( Ethernet: MAC management counters ) 
    Ethernet_MMC $0 + constant Ethernet_MMC_MMCCR ( read-write )  \ Ethernet MMC control register
    Ethernet_MMC $4 + constant Ethernet_MMC_MMCRIR ( read-write )  \ Ethernet MMC receive interrupt register
    Ethernet_MMC $8 + constant Ethernet_MMC_MMCTIR ( read-only )  \ Ethernet MMC transmit interrupt register
    Ethernet_MMC $C + constant Ethernet_MMC_MMCRIMR ( read-write )  \ Ethernet MMC receive interrupt mask register
    Ethernet_MMC $10 + constant Ethernet_MMC_MMCTIMR ( read-write )  \ Ethernet MMC transmit interrupt mask register
    Ethernet_MMC $4C + constant Ethernet_MMC_MMCTGFSCCR ( read-only )  \ Ethernet MMC transmitted good frames after a single collision counter
    Ethernet_MMC $50 + constant Ethernet_MMC_MMCTGFMSCCR ( read-only )  \ Ethernet MMC transmitted good frames after more than a single collision
    Ethernet_MMC $68 + constant Ethernet_MMC_MMCTGFCR ( read-only )  \ Ethernet MMC transmitted good frames counter register
    Ethernet_MMC $94 + constant Ethernet_MMC_MMCRFCECR ( read-only )  \ Ethernet MMC received frames with CRC error counter register
    Ethernet_MMC $98 + constant Ethernet_MMC_MMCRFAECR ( read-only )  \ Ethernet MMC received frames with alignment error counter register
    Ethernet_MMC $C4 + constant Ethernet_MMC_MMCRGUFCR ( read-only )  \ MMC received good unicast frames counter register
    : Ethernet_MMC_MMCCR. cr ." Ethernet_MMC_MMCCR.  RW   $" Ethernet_MMC_MMCCR @ hex. Ethernet_MMC_MMCCR 1b. ;
    : Ethernet_MMC_MMCRIR. cr ." Ethernet_MMC_MMCRIR.  RW   $" Ethernet_MMC_MMCRIR @ hex. Ethernet_MMC_MMCRIR 1b. ;
    : Ethernet_MMC_MMCTIR. cr ." Ethernet_MMC_MMCTIR.  RO   $" Ethernet_MMC_MMCTIR @ hex. Ethernet_MMC_MMCTIR 1b. ;
    : Ethernet_MMC_MMCRIMR. cr ." Ethernet_MMC_MMCRIMR.  RW   $" Ethernet_MMC_MMCRIMR @ hex. Ethernet_MMC_MMCRIMR 1b. ;
    : Ethernet_MMC_MMCTIMR. cr ." Ethernet_MMC_MMCTIMR.  RW   $" Ethernet_MMC_MMCTIMR @ hex. Ethernet_MMC_MMCTIMR 1b. ;
    : Ethernet_MMC_MMCTGFSCCR. cr ." Ethernet_MMC_MMCTGFSCCR.  RO   $" Ethernet_MMC_MMCTGFSCCR @ hex. Ethernet_MMC_MMCTGFSCCR 1b. ;
    : Ethernet_MMC_MMCTGFMSCCR. cr ." Ethernet_MMC_MMCTGFMSCCR.  RO   $" Ethernet_MMC_MMCTGFMSCCR @ hex. Ethernet_MMC_MMCTGFMSCCR 1b. ;
    : Ethernet_MMC_MMCTGFCR. cr ." Ethernet_MMC_MMCTGFCR.  RO   $" Ethernet_MMC_MMCTGFCR @ hex. Ethernet_MMC_MMCTGFCR 1b. ;
    : Ethernet_MMC_MMCRFCECR. cr ." Ethernet_MMC_MMCRFCECR.  RO   $" Ethernet_MMC_MMCRFCECR @ hex. Ethernet_MMC_MMCRFCECR 1b. ;
    : Ethernet_MMC_MMCRFAECR. cr ." Ethernet_MMC_MMCRFAECR.  RO   $" Ethernet_MMC_MMCRFAECR @ hex. Ethernet_MMC_MMCRFAECR 1b. ;
    : Ethernet_MMC_MMCRGUFCR. cr ." Ethernet_MMC_MMCRGUFCR.  RO   $" Ethernet_MMC_MMCRGUFCR @ hex. Ethernet_MMC_MMCRGUFCR 1b. ;
    : Ethernet_MMC.
      Ethernet_MMC_MMCCR.
      Ethernet_MMC_MMCRIR.
      Ethernet_MMC_MMCTIR.
      Ethernet_MMC_MMCRIMR.
      Ethernet_MMC_MMCTIMR.
      Ethernet_MMC_MMCTGFSCCR.
      Ethernet_MMC_MMCTGFMSCCR.
      Ethernet_MMC_MMCTGFCR.
      Ethernet_MMC_MMCRFCECR.
      Ethernet_MMC_MMCRFAECR.
      Ethernet_MMC_MMCRGUFCR.
    ;
  [then]

  execute-defined? use-Ethernet_PTP [if]
    $40028700 constant Ethernet_PTP ( Ethernet: Precision time protocol ) 
    Ethernet_PTP $0 + constant Ethernet_PTP_PTPTSCR ( read-write )  \ Ethernet PTP time stamp control register
    Ethernet_PTP $4 + constant Ethernet_PTP_PTPSSIR ( read-write )  \ Ethernet PTP subsecond increment register
    Ethernet_PTP $8 + constant Ethernet_PTP_PTPTSHR ( read-only )  \ Ethernet PTP time stamp high register
    Ethernet_PTP $C + constant Ethernet_PTP_PTPTSLR ( read-only )  \ Ethernet PTP time stamp low register
    Ethernet_PTP $10 + constant Ethernet_PTP_PTPTSHUR ( read-write )  \ Ethernet PTP time stamp high update register
    Ethernet_PTP $14 + constant Ethernet_PTP_PTPTSLUR ( read-write )  \ Ethernet PTP time stamp low update register
    Ethernet_PTP $18 + constant Ethernet_PTP_PTPTSAR ( read-write )  \ Ethernet PTP time stamp addend register
    Ethernet_PTP $1C + constant Ethernet_PTP_PTPTTHR ( read-write )  \ Ethernet PTP target time high register
    Ethernet_PTP $20 + constant Ethernet_PTP_PTPTTLR ( read-write )  \ Ethernet PTP target time low register
    Ethernet_PTP $28 + constant Ethernet_PTP_PTPTSSR ( read-only )  \ Ethernet PTP time stamp status register
    Ethernet_PTP $2C + constant Ethernet_PTP_PTPPPSCR ( read-only )  \ Ethernet PTP PPS control register
    : Ethernet_PTP_PTPTSCR. cr ." Ethernet_PTP_PTPTSCR.  RW   $" Ethernet_PTP_PTPTSCR @ hex. Ethernet_PTP_PTPTSCR 1b. ;
    : Ethernet_PTP_PTPSSIR. cr ." Ethernet_PTP_PTPSSIR.  RW   $" Ethernet_PTP_PTPSSIR @ hex. Ethernet_PTP_PTPSSIR 1b. ;
    : Ethernet_PTP_PTPTSHR. cr ." Ethernet_PTP_PTPTSHR.  RO   $" Ethernet_PTP_PTPTSHR @ hex. Ethernet_PTP_PTPTSHR 1b. ;
    : Ethernet_PTP_PTPTSLR. cr ." Ethernet_PTP_PTPTSLR.  RO   $" Ethernet_PTP_PTPTSLR @ hex. Ethernet_PTP_PTPTSLR 1b. ;
    : Ethernet_PTP_PTPTSHUR. cr ." Ethernet_PTP_PTPTSHUR.  RW   $" Ethernet_PTP_PTPTSHUR @ hex. Ethernet_PTP_PTPTSHUR 1b. ;
    : Ethernet_PTP_PTPTSLUR. cr ." Ethernet_PTP_PTPTSLUR.  RW   $" Ethernet_PTP_PTPTSLUR @ hex. Ethernet_PTP_PTPTSLUR 1b. ;
    : Ethernet_PTP_PTPTSAR. cr ." Ethernet_PTP_PTPTSAR.  RW   $" Ethernet_PTP_PTPTSAR @ hex. Ethernet_PTP_PTPTSAR 1b. ;
    : Ethernet_PTP_PTPTTHR. cr ." Ethernet_PTP_PTPTTHR.  RW   $" Ethernet_PTP_PTPTTHR @ hex. Ethernet_PTP_PTPTTHR 1b. ;
    : Ethernet_PTP_PTPTTLR. cr ." Ethernet_PTP_PTPTTLR.  RW   $" Ethernet_PTP_PTPTTLR @ hex. Ethernet_PTP_PTPTTLR 1b. ;
    : Ethernet_PTP_PTPTSSR. cr ." Ethernet_PTP_PTPTSSR.  RO   $" Ethernet_PTP_PTPTSSR @ hex. Ethernet_PTP_PTPTSSR 1b. ;
    : Ethernet_PTP_PTPPPSCR. cr ." Ethernet_PTP_PTPPPSCR.  RO   $" Ethernet_PTP_PTPPPSCR @ hex. Ethernet_PTP_PTPPPSCR 1b. ;
    : Ethernet_PTP.
      Ethernet_PTP_PTPTSCR.
      Ethernet_PTP_PTPSSIR.
      Ethernet_PTP_PTPTSHR.
      Ethernet_PTP_PTPTSLR.
      Ethernet_PTP_PTPTSHUR.
      Ethernet_PTP_PTPTSLUR.
      Ethernet_PTP_PTPTSAR.
      Ethernet_PTP_PTPTTHR.
      Ethernet_PTP_PTPTTLR.
      Ethernet_PTP_PTPTSSR.
      Ethernet_PTP_PTPPPSCR.
    ;
  [then]

  execute-defined? use-Ethernet_DMA [if]
    $40029000 constant Ethernet_DMA ( Ethernet: DMA controller operation ) 
    Ethernet_DMA $0 + constant Ethernet_DMA_DMABMR ( read-write )  \ Ethernet DMA bus mode register
    Ethernet_DMA $4 + constant Ethernet_DMA_DMATPDR ( read-write )  \ Ethernet DMA transmit poll demand register
    Ethernet_DMA $8 + constant Ethernet_DMA_DMARPDR ( read-write )  \ EHERNET DMA receive poll demand register
    Ethernet_DMA $C + constant Ethernet_DMA_DMARDLAR ( read-write )  \ Ethernet DMA receive descriptor list address register
    Ethernet_DMA $10 + constant Ethernet_DMA_DMATDLAR ( read-write )  \ Ethernet DMA transmit descriptor list address register
    Ethernet_DMA $14 + constant Ethernet_DMA_DMASR (  )  \ Ethernet DMA status register
    Ethernet_DMA $18 + constant Ethernet_DMA_DMAOMR ( read-write )  \ Ethernet DMA operation mode register
    Ethernet_DMA $1C + constant Ethernet_DMA_DMAIER ( read-write )  \ Ethernet DMA interrupt enable register
    Ethernet_DMA $20 + constant Ethernet_DMA_DMAMFBOCR ( read-write )  \ Ethernet DMA missed frame and buffer overflow counter register
    Ethernet_DMA $24 + constant Ethernet_DMA_DMARSWTR ( read-write )  \ Ethernet DMA receive status watchdog timer register
    Ethernet_DMA $48 + constant Ethernet_DMA_DMACHTDR ( read-only )  \ Ethernet DMA current host transmit descriptor register
    Ethernet_DMA $4C + constant Ethernet_DMA_DMACHRDR ( read-only )  \ Ethernet DMA current host receive descriptor register
    Ethernet_DMA $50 + constant Ethernet_DMA_DMACHTBAR ( read-only )  \ Ethernet DMA current host transmit buffer address register
    Ethernet_DMA $54 + constant Ethernet_DMA_DMACHRBAR ( read-only )  \ Ethernet DMA current host receive buffer address register
    : Ethernet_DMA_DMABMR. cr ." Ethernet_DMA_DMABMR.  RW   $" Ethernet_DMA_DMABMR @ hex. Ethernet_DMA_DMABMR 1b. ;
    : Ethernet_DMA_DMATPDR. cr ." Ethernet_DMA_DMATPDR.  RW   $" Ethernet_DMA_DMATPDR @ hex. Ethernet_DMA_DMATPDR 1b. ;
    : Ethernet_DMA_DMARPDR. cr ." Ethernet_DMA_DMARPDR.  RW   $" Ethernet_DMA_DMARPDR @ hex. Ethernet_DMA_DMARPDR 1b. ;
    : Ethernet_DMA_DMARDLAR. cr ." Ethernet_DMA_DMARDLAR.  RW   $" Ethernet_DMA_DMARDLAR @ hex. Ethernet_DMA_DMARDLAR 1b. ;
    : Ethernet_DMA_DMATDLAR. cr ." Ethernet_DMA_DMATDLAR.  RW   $" Ethernet_DMA_DMATDLAR @ hex. Ethernet_DMA_DMATDLAR 1b. ;
    : Ethernet_DMA_DMASR. cr ." Ethernet_DMA_DMASR.   $" Ethernet_DMA_DMASR @ hex. Ethernet_DMA_DMASR 1b. ;
    : Ethernet_DMA_DMAOMR. cr ." Ethernet_DMA_DMAOMR.  RW   $" Ethernet_DMA_DMAOMR @ hex. Ethernet_DMA_DMAOMR 1b. ;
    : Ethernet_DMA_DMAIER. cr ." Ethernet_DMA_DMAIER.  RW   $" Ethernet_DMA_DMAIER @ hex. Ethernet_DMA_DMAIER 1b. ;
    : Ethernet_DMA_DMAMFBOCR. cr ." Ethernet_DMA_DMAMFBOCR.  RW   $" Ethernet_DMA_DMAMFBOCR @ hex. Ethernet_DMA_DMAMFBOCR 1b. ;
    : Ethernet_DMA_DMARSWTR. cr ." Ethernet_DMA_DMARSWTR.  RW   $" Ethernet_DMA_DMARSWTR @ hex. Ethernet_DMA_DMARSWTR 1b. ;
    : Ethernet_DMA_DMACHTDR. cr ." Ethernet_DMA_DMACHTDR.  RO   $" Ethernet_DMA_DMACHTDR @ hex. Ethernet_DMA_DMACHTDR 1b. ;
    : Ethernet_DMA_DMACHRDR. cr ." Ethernet_DMA_DMACHRDR.  RO   $" Ethernet_DMA_DMACHRDR @ hex. Ethernet_DMA_DMACHRDR 1b. ;
    : Ethernet_DMA_DMACHTBAR. cr ." Ethernet_DMA_DMACHTBAR.  RO   $" Ethernet_DMA_DMACHTBAR @ hex. Ethernet_DMA_DMACHTBAR 1b. ;
    : Ethernet_DMA_DMACHRBAR. cr ." Ethernet_DMA_DMACHRBAR.  RO   $" Ethernet_DMA_DMACHRBAR @ hex. Ethernet_DMA_DMACHRBAR 1b. ;
    : Ethernet_DMA.
      Ethernet_DMA_DMABMR.
      Ethernet_DMA_DMATPDR.
      Ethernet_DMA_DMARPDR.
      Ethernet_DMA_DMARDLAR.
      Ethernet_DMA_DMATDLAR.
      Ethernet_DMA_DMASR.
      Ethernet_DMA_DMAOMR.
      Ethernet_DMA_DMAIER.
      Ethernet_DMA_DMAMFBOCR.
      Ethernet_DMA_DMARSWTR.
      Ethernet_DMA_DMACHTDR.
      Ethernet_DMA_DMACHRDR.
      Ethernet_DMA_DMACHTBAR.
      Ethernet_DMA_DMACHRBAR.
    ;
  [then]

  execute-defined? use-CRC [if]
    $40023000 constant CRC ( Cryptographic processor ) 
    CRC $0 + constant CRC_DR ( read-write )  \ Data register
    CRC $4 + constant CRC_IDR ( read-write )  \ Independent Data register
    CRC $8 + constant CRC_CR ( write-only )  \ Control register
    CRC $10 + constant CRC_INIT ( read-write )  \ Initial CRC value
    CRC $14 + constant CRC_POL ( read-write )  \ CRC polynomial
    : CRC_DR. cr ." CRC_DR.  RW   $" CRC_DR @ hex. CRC_DR 1b. ;
    : CRC_IDR. cr ." CRC_IDR.  RW   $" CRC_IDR @ hex. CRC_IDR 1b. ;
    : CRC_CR. cr ." CRC_CR " WRITEONLY ; 
    : CRC_INIT. cr ." CRC_INIT.  RW   $" CRC_INIT @ hex. CRC_INIT 1b. ;
    : CRC_POL. cr ." CRC_POL.  RW   $" CRC_POL @ hex. CRC_POL 1b. ;
    : CRC.
      CRC_DR.
      CRC_IDR.
      CRC_CR.
      CRC_INIT.
      CRC_POL.
    ;
  [then]

  execute-defined? use-CAN1 [if]
    $40006400 constant CAN1 ( Controller area network ) 
    CAN1 $0 + constant CAN1_MCR ( read-write )  \ master control register
    CAN1 $4 + constant CAN1_MSR (  )  \ master status register
    CAN1 $8 + constant CAN1_TSR (  )  \ transmit status register
    CAN1 $C + constant CAN1_RF0R (  )  \ receive FIFO 0 register
    CAN1 $10 + constant CAN1_RF1R (  )  \ receive FIFO 1 register
    CAN1 $14 + constant CAN1_IER ( read-write )  \ interrupt enable register
    CAN1 $18 + constant CAN1_ESR (  )  \ interrupt enable register
    CAN1 $1C + constant CAN1_BTR ( read-write )  \ bit timing register
    CAN1 $180 + constant CAN1_TI0R ( read-write )  \ TX mailbox identifier register
    CAN1 $184 + constant CAN1_TDT0R ( read-write )  \ mailbox data length control and time stamp register
    CAN1 $188 + constant CAN1_TDL0R ( read-write )  \ mailbox data low register
    CAN1 $18C + constant CAN1_TDH0R ( read-write )  \ mailbox data high register
    CAN1 $190 + constant CAN1_TI1R ( read-write )  \ mailbox identifier register
    CAN1 $194 + constant CAN1_TDT1R ( read-write )  \ mailbox data length control and time stamp register
    CAN1 $198 + constant CAN1_TDL1R ( read-write )  \ mailbox data low register
    CAN1 $19C + constant CAN1_TDH1R ( read-write )  \ mailbox data high register
    CAN1 $1A0 + constant CAN1_TI2R ( read-write )  \ mailbox identifier register
    CAN1 $1A4 + constant CAN1_TDT2R ( read-write )  \ mailbox data length control and time stamp register
    CAN1 $1A8 + constant CAN1_TDL2R ( read-write )  \ mailbox data low register
    CAN1 $1AC + constant CAN1_TDH2R ( read-write )  \ mailbox data high register
    CAN1 $1B0 + constant CAN1_RI0R ( read-only )  \ receive FIFO mailbox identifier register
    CAN1 $1B4 + constant CAN1_RDT0R ( read-only )  \ mailbox data high register
    CAN1 $1B8 + constant CAN1_RDL0R ( read-only )  \ mailbox data high register
    CAN1 $1BC + constant CAN1_RDH0R ( read-only )  \ receive FIFO mailbox data high register
    CAN1 $1C0 + constant CAN1_RI1R ( read-only )  \ mailbox data high register
    CAN1 $1C4 + constant CAN1_RDT1R ( read-only )  \ mailbox data high register
    CAN1 $1C8 + constant CAN1_RDL1R ( read-only )  \ mailbox data high register
    CAN1 $1CC + constant CAN1_RDH1R ( read-only )  \ mailbox data high register
    CAN1 $200 + constant CAN1_FMR ( read-write )  \ filter master register
    CAN1 $204 + constant CAN1_FM1R ( read-write )  \ filter mode register
    CAN1 $20C + constant CAN1_FS1R ( read-write )  \ filter scale register
    CAN1 $214 + constant CAN1_FFA1R ( read-write )  \ filter FIFO assignment register
    CAN1 $21C + constant CAN1_FA1R ( read-write )  \ filter activation register
    CAN1 $240 + constant CAN1_F0R1 ( read-write )  \ Filter bank 0 register 1
    CAN1 $244 + constant CAN1_F0R2 ( read-write )  \ Filter bank 0 register 2
    CAN1 $248 + constant CAN1_F1R1 ( read-write )  \ Filter bank 1 register 1
    CAN1 $24C + constant CAN1_F1R2 ( read-write )  \ Filter bank 1 register 2
    CAN1 $250 + constant CAN1_F2R1 ( read-write )  \ Filter bank 2 register 1
    CAN1 $254 + constant CAN1_F2R2 ( read-write )  \ Filter bank 2 register 2
    CAN1 $258 + constant CAN1_F3R1 ( read-write )  \ Filter bank 3 register 1
    CAN1 $25C + constant CAN1_F3R2 ( read-write )  \ Filter bank 3 register 2
    CAN1 $260 + constant CAN1_F4R1 ( read-write )  \ Filter bank 4 register 1
    CAN1 $264 + constant CAN1_F4R2 ( read-write )  \ Filter bank 4 register 2
    CAN1 $268 + constant CAN1_F5R1 ( read-write )  \ Filter bank 5 register 1
    CAN1 $26C + constant CAN1_F5R2 ( read-write )  \ Filter bank 5 register 2
    CAN1 $270 + constant CAN1_F6R1 ( read-write )  \ Filter bank 6 register 1
    CAN1 $274 + constant CAN1_F6R2 ( read-write )  \ Filter bank 6 register 2
    CAN1 $278 + constant CAN1_F7R1 ( read-write )  \ Filter bank 7 register 1
    CAN1 $27C + constant CAN1_F7R2 ( read-write )  \ Filter bank 7 register 2
    CAN1 $280 + constant CAN1_F8R1 ( read-write )  \ Filter bank 8 register 1
    CAN1 $284 + constant CAN1_F8R2 ( read-write )  \ Filter bank 8 register 2
    CAN1 $288 + constant CAN1_F9R1 ( read-write )  \ Filter bank 9 register 1
    CAN1 $28C + constant CAN1_F9R2 ( read-write )  \ Filter bank 9 register 2
    CAN1 $290 + constant CAN1_F10R1 ( read-write )  \ Filter bank 10 register 1
    CAN1 $294 + constant CAN1_F10R2 ( read-write )  \ Filter bank 10 register 2
    CAN1 $298 + constant CAN1_F11R1 ( read-write )  \ Filter bank 11 register 1
    CAN1 $29C + constant CAN1_F11R2 ( read-write )  \ Filter bank 11 register 2
    CAN1 $2A0 + constant CAN1_F12R1 ( read-write )  \ Filter bank 4 register 1
    CAN1 $2A4 + constant CAN1_F12R2 ( read-write )  \ Filter bank 12 register 2
    CAN1 $2A8 + constant CAN1_F13R1 ( read-write )  \ Filter bank 13 register 1
    CAN1 $2AC + constant CAN1_F13R2 ( read-write )  \ Filter bank 13 register 2
    CAN1 $2B0 + constant CAN1_F14R1 ( read-write )  \ Filter bank 14 register 1
    CAN1 $2B4 + constant CAN1_F14R2 ( read-write )  \ Filter bank 14 register 2
    CAN1 $2B8 + constant CAN1_F15R1 ( read-write )  \ Filter bank 15 register 1
    CAN1 $2BC + constant CAN1_F15R2 ( read-write )  \ Filter bank 15 register 2
    CAN1 $2C0 + constant CAN1_F16R1 ( read-write )  \ Filter bank 16 register 1
    CAN1 $2C4 + constant CAN1_F16R2 ( read-write )  \ Filter bank 16 register 2
    CAN1 $2C8 + constant CAN1_F17R1 ( read-write )  \ Filter bank 17 register 1
    CAN1 $2CC + constant CAN1_F17R2 ( read-write )  \ Filter bank 17 register 2
    CAN1 $2D0 + constant CAN1_F18R1 ( read-write )  \ Filter bank 18 register 1
    CAN1 $2D4 + constant CAN1_F18R2 ( read-write )  \ Filter bank 18 register 2
    CAN1 $2D8 + constant CAN1_F19R1 ( read-write )  \ Filter bank 19 register 1
    CAN1 $2DC + constant CAN1_F19R2 ( read-write )  \ Filter bank 19 register 2
    CAN1 $2E0 + constant CAN1_F20R1 ( read-write )  \ Filter bank 20 register 1
    CAN1 $2E4 + constant CAN1_F20R2 ( read-write )  \ Filter bank 20 register 2
    CAN1 $2E8 + constant CAN1_F21R1 ( read-write )  \ Filter bank 21 register 1
    CAN1 $2EC + constant CAN1_F21R2 ( read-write )  \ Filter bank 21 register 2
    CAN1 $2F0 + constant CAN1_F22R1 ( read-write )  \ Filter bank 22 register 1
    CAN1 $2F4 + constant CAN1_F22R2 ( read-write )  \ Filter bank 22 register 2
    CAN1 $2F8 + constant CAN1_F23R1 ( read-write )  \ Filter bank 23 register 1
    CAN1 $2FC + constant CAN1_F23R2 ( read-write )  \ Filter bank 23 register 2
    CAN1 $300 + constant CAN1_F24R1 ( read-write )  \ Filter bank 24 register 1
    CAN1 $304 + constant CAN1_F24R2 ( read-write )  \ Filter bank 24 register 2
    CAN1 $308 + constant CAN1_F25R1 ( read-write )  \ Filter bank 25 register 1
    CAN1 $30C + constant CAN1_F25R2 ( read-write )  \ Filter bank 25 register 2
    CAN1 $310 + constant CAN1_F26R1 ( read-write )  \ Filter bank 26 register 1
    CAN1 $314 + constant CAN1_F26R2 ( read-write )  \ Filter bank 26 register 2
    CAN1 $318 + constant CAN1_F27R1 ( read-write )  \ Filter bank 27 register 1
    CAN1 $31C + constant CAN1_F27R2 ( read-write )  \ Filter bank 27 register 2
    : CAN1_MCR. cr ." CAN1_MCR.  RW   $" CAN1_MCR @ hex. CAN1_MCR 1b. ;
    : CAN1_MSR. cr ." CAN1_MSR.   $" CAN1_MSR @ hex. CAN1_MSR 1b. ;
    : CAN1_TSR. cr ." CAN1_TSR.   $" CAN1_TSR @ hex. CAN1_TSR 1b. ;
    : CAN1_RF0R. cr ." CAN1_RF0R.   $" CAN1_RF0R @ hex. CAN1_RF0R 1b. ;
    : CAN1_RF1R. cr ." CAN1_RF1R.   $" CAN1_RF1R @ hex. CAN1_RF1R 1b. ;
    : CAN1_IER. cr ." CAN1_IER.  RW   $" CAN1_IER @ hex. CAN1_IER 1b. ;
    : CAN1_ESR. cr ." CAN1_ESR.   $" CAN1_ESR @ hex. CAN1_ESR 1b. ;
    : CAN1_BTR. cr ." CAN1_BTR.  RW   $" CAN1_BTR @ hex. CAN1_BTR 1b. ;
    : CAN1_TI0R. cr ." CAN1_TI0R.  RW   $" CAN1_TI0R @ hex. CAN1_TI0R 1b. ;
    : CAN1_TDT0R. cr ." CAN1_TDT0R.  RW   $" CAN1_TDT0R @ hex. CAN1_TDT0R 1b. ;
    : CAN1_TDL0R. cr ." CAN1_TDL0R.  RW   $" CAN1_TDL0R @ hex. CAN1_TDL0R 1b. ;
    : CAN1_TDH0R. cr ." CAN1_TDH0R.  RW   $" CAN1_TDH0R @ hex. CAN1_TDH0R 1b. ;
    : CAN1_TI1R. cr ." CAN1_TI1R.  RW   $" CAN1_TI1R @ hex. CAN1_TI1R 1b. ;
    : CAN1_TDT1R. cr ." CAN1_TDT1R.  RW   $" CAN1_TDT1R @ hex. CAN1_TDT1R 1b. ;
    : CAN1_TDL1R. cr ." CAN1_TDL1R.  RW   $" CAN1_TDL1R @ hex. CAN1_TDL1R 1b. ;
    : CAN1_TDH1R. cr ." CAN1_TDH1R.  RW   $" CAN1_TDH1R @ hex. CAN1_TDH1R 1b. ;
    : CAN1_TI2R. cr ." CAN1_TI2R.  RW   $" CAN1_TI2R @ hex. CAN1_TI2R 1b. ;
    : CAN1_TDT2R. cr ." CAN1_TDT2R.  RW   $" CAN1_TDT2R @ hex. CAN1_TDT2R 1b. ;
    : CAN1_TDL2R. cr ." CAN1_TDL2R.  RW   $" CAN1_TDL2R @ hex. CAN1_TDL2R 1b. ;
    : CAN1_TDH2R. cr ." CAN1_TDH2R.  RW   $" CAN1_TDH2R @ hex. CAN1_TDH2R 1b. ;
    : CAN1_RI0R. cr ." CAN1_RI0R.  RO   $" CAN1_RI0R @ hex. CAN1_RI0R 1b. ;
    : CAN1_RDT0R. cr ." CAN1_RDT0R.  RO   $" CAN1_RDT0R @ hex. CAN1_RDT0R 1b. ;
    : CAN1_RDL0R. cr ." CAN1_RDL0R.  RO   $" CAN1_RDL0R @ hex. CAN1_RDL0R 1b. ;
    : CAN1_RDH0R. cr ." CAN1_RDH0R.  RO   $" CAN1_RDH0R @ hex. CAN1_RDH0R 1b. ;
    : CAN1_RI1R. cr ." CAN1_RI1R.  RO   $" CAN1_RI1R @ hex. CAN1_RI1R 1b. ;
    : CAN1_RDT1R. cr ." CAN1_RDT1R.  RO   $" CAN1_RDT1R @ hex. CAN1_RDT1R 1b. ;
    : CAN1_RDL1R. cr ." CAN1_RDL1R.  RO   $" CAN1_RDL1R @ hex. CAN1_RDL1R 1b. ;
    : CAN1_RDH1R. cr ." CAN1_RDH1R.  RO   $" CAN1_RDH1R @ hex. CAN1_RDH1R 1b. ;
    : CAN1_FMR. cr ." CAN1_FMR.  RW   $" CAN1_FMR @ hex. CAN1_FMR 1b. ;
    : CAN1_FM1R. cr ." CAN1_FM1R.  RW   $" CAN1_FM1R @ hex. CAN1_FM1R 1b. ;
    : CAN1_FS1R. cr ." CAN1_FS1R.  RW   $" CAN1_FS1R @ hex. CAN1_FS1R 1b. ;
    : CAN1_FFA1R. cr ." CAN1_FFA1R.  RW   $" CAN1_FFA1R @ hex. CAN1_FFA1R 1b. ;
    : CAN1_FA1R. cr ." CAN1_FA1R.  RW   $" CAN1_FA1R @ hex. CAN1_FA1R 1b. ;
    : CAN1_F0R1. cr ." CAN1_F0R1.  RW   $" CAN1_F0R1 @ hex. CAN1_F0R1 1b. ;
    : CAN1_F0R2. cr ." CAN1_F0R2.  RW   $" CAN1_F0R2 @ hex. CAN1_F0R2 1b. ;
    : CAN1_F1R1. cr ." CAN1_F1R1.  RW   $" CAN1_F1R1 @ hex. CAN1_F1R1 1b. ;
    : CAN1_F1R2. cr ." CAN1_F1R2.  RW   $" CAN1_F1R2 @ hex. CAN1_F1R2 1b. ;
    : CAN1_F2R1. cr ." CAN1_F2R1.  RW   $" CAN1_F2R1 @ hex. CAN1_F2R1 1b. ;
    : CAN1_F2R2. cr ." CAN1_F2R2.  RW   $" CAN1_F2R2 @ hex. CAN1_F2R2 1b. ;
    : CAN1_F3R1. cr ." CAN1_F3R1.  RW   $" CAN1_F3R1 @ hex. CAN1_F3R1 1b. ;
    : CAN1_F3R2. cr ." CAN1_F3R2.  RW   $" CAN1_F3R2 @ hex. CAN1_F3R2 1b. ;
    : CAN1_F4R1. cr ." CAN1_F4R1.  RW   $" CAN1_F4R1 @ hex. CAN1_F4R1 1b. ;
    : CAN1_F4R2. cr ." CAN1_F4R2.  RW   $" CAN1_F4R2 @ hex. CAN1_F4R2 1b. ;
    : CAN1_F5R1. cr ." CAN1_F5R1.  RW   $" CAN1_F5R1 @ hex. CAN1_F5R1 1b. ;
    : CAN1_F5R2. cr ." CAN1_F5R2.  RW   $" CAN1_F5R2 @ hex. CAN1_F5R2 1b. ;
    : CAN1_F6R1. cr ." CAN1_F6R1.  RW   $" CAN1_F6R1 @ hex. CAN1_F6R1 1b. ;
    : CAN1_F6R2. cr ." CAN1_F6R2.  RW   $" CAN1_F6R2 @ hex. CAN1_F6R2 1b. ;
    : CAN1_F7R1. cr ." CAN1_F7R1.  RW   $" CAN1_F7R1 @ hex. CAN1_F7R1 1b. ;
    : CAN1_F7R2. cr ." CAN1_F7R2.  RW   $" CAN1_F7R2 @ hex. CAN1_F7R2 1b. ;
    : CAN1_F8R1. cr ." CAN1_F8R1.  RW   $" CAN1_F8R1 @ hex. CAN1_F8R1 1b. ;
    : CAN1_F8R2. cr ." CAN1_F8R2.  RW   $" CAN1_F8R2 @ hex. CAN1_F8R2 1b. ;
    : CAN1_F9R1. cr ." CAN1_F9R1.  RW   $" CAN1_F9R1 @ hex. CAN1_F9R1 1b. ;
    : CAN1_F9R2. cr ." CAN1_F9R2.  RW   $" CAN1_F9R2 @ hex. CAN1_F9R2 1b. ;
    : CAN1_F10R1. cr ." CAN1_F10R1.  RW   $" CAN1_F10R1 @ hex. CAN1_F10R1 1b. ;
    : CAN1_F10R2. cr ." CAN1_F10R2.  RW   $" CAN1_F10R2 @ hex. CAN1_F10R2 1b. ;
    : CAN1_F11R1. cr ." CAN1_F11R1.  RW   $" CAN1_F11R1 @ hex. CAN1_F11R1 1b. ;
    : CAN1_F11R2. cr ." CAN1_F11R2.  RW   $" CAN1_F11R2 @ hex. CAN1_F11R2 1b. ;
    : CAN1_F12R1. cr ." CAN1_F12R1.  RW   $" CAN1_F12R1 @ hex. CAN1_F12R1 1b. ;
    : CAN1_F12R2. cr ." CAN1_F12R2.  RW   $" CAN1_F12R2 @ hex. CAN1_F12R2 1b. ;
    : CAN1_F13R1. cr ." CAN1_F13R1.  RW   $" CAN1_F13R1 @ hex. CAN1_F13R1 1b. ;
    : CAN1_F13R2. cr ." CAN1_F13R2.  RW   $" CAN1_F13R2 @ hex. CAN1_F13R2 1b. ;
    : CAN1_F14R1. cr ." CAN1_F14R1.  RW   $" CAN1_F14R1 @ hex. CAN1_F14R1 1b. ;
    : CAN1_F14R2. cr ." CAN1_F14R2.  RW   $" CAN1_F14R2 @ hex. CAN1_F14R2 1b. ;
    : CAN1_F15R1. cr ." CAN1_F15R1.  RW   $" CAN1_F15R1 @ hex. CAN1_F15R1 1b. ;
    : CAN1_F15R2. cr ." CAN1_F15R2.  RW   $" CAN1_F15R2 @ hex. CAN1_F15R2 1b. ;
    : CAN1_F16R1. cr ." CAN1_F16R1.  RW   $" CAN1_F16R1 @ hex. CAN1_F16R1 1b. ;
    : CAN1_F16R2. cr ." CAN1_F16R2.  RW   $" CAN1_F16R2 @ hex. CAN1_F16R2 1b. ;
    : CAN1_F17R1. cr ." CAN1_F17R1.  RW   $" CAN1_F17R1 @ hex. CAN1_F17R1 1b. ;
    : CAN1_F17R2. cr ." CAN1_F17R2.  RW   $" CAN1_F17R2 @ hex. CAN1_F17R2 1b. ;
    : CAN1_F18R1. cr ." CAN1_F18R1.  RW   $" CAN1_F18R1 @ hex. CAN1_F18R1 1b. ;
    : CAN1_F18R2. cr ." CAN1_F18R2.  RW   $" CAN1_F18R2 @ hex. CAN1_F18R2 1b. ;
    : CAN1_F19R1. cr ." CAN1_F19R1.  RW   $" CAN1_F19R1 @ hex. CAN1_F19R1 1b. ;
    : CAN1_F19R2. cr ." CAN1_F19R2.  RW   $" CAN1_F19R2 @ hex. CAN1_F19R2 1b. ;
    : CAN1_F20R1. cr ." CAN1_F20R1.  RW   $" CAN1_F20R1 @ hex. CAN1_F20R1 1b. ;
    : CAN1_F20R2. cr ." CAN1_F20R2.  RW   $" CAN1_F20R2 @ hex. CAN1_F20R2 1b. ;
    : CAN1_F21R1. cr ." CAN1_F21R1.  RW   $" CAN1_F21R1 @ hex. CAN1_F21R1 1b. ;
    : CAN1_F21R2. cr ." CAN1_F21R2.  RW   $" CAN1_F21R2 @ hex. CAN1_F21R2 1b. ;
    : CAN1_F22R1. cr ." CAN1_F22R1.  RW   $" CAN1_F22R1 @ hex. CAN1_F22R1 1b. ;
    : CAN1_F22R2. cr ." CAN1_F22R2.  RW   $" CAN1_F22R2 @ hex. CAN1_F22R2 1b. ;
    : CAN1_F23R1. cr ." CAN1_F23R1.  RW   $" CAN1_F23R1 @ hex. CAN1_F23R1 1b. ;
    : CAN1_F23R2. cr ." CAN1_F23R2.  RW   $" CAN1_F23R2 @ hex. CAN1_F23R2 1b. ;
    : CAN1_F24R1. cr ." CAN1_F24R1.  RW   $" CAN1_F24R1 @ hex. CAN1_F24R1 1b. ;
    : CAN1_F24R2. cr ." CAN1_F24R2.  RW   $" CAN1_F24R2 @ hex. CAN1_F24R2 1b. ;
    : CAN1_F25R1. cr ." CAN1_F25R1.  RW   $" CAN1_F25R1 @ hex. CAN1_F25R1 1b. ;
    : CAN1_F25R2. cr ." CAN1_F25R2.  RW   $" CAN1_F25R2 @ hex. CAN1_F25R2 1b. ;
    : CAN1_F26R1. cr ." CAN1_F26R1.  RW   $" CAN1_F26R1 @ hex. CAN1_F26R1 1b. ;
    : CAN1_F26R2. cr ." CAN1_F26R2.  RW   $" CAN1_F26R2 @ hex. CAN1_F26R2 1b. ;
    : CAN1_F27R1. cr ." CAN1_F27R1.  RW   $" CAN1_F27R1 @ hex. CAN1_F27R1 1b. ;
    : CAN1_F27R2. cr ." CAN1_F27R2.  RW   $" CAN1_F27R2 @ hex. CAN1_F27R2 1b. ;
    : CAN1.
      CAN1_MCR.
      CAN1_MSR.
      CAN1_TSR.
      CAN1_RF0R.
      CAN1_RF1R.
      CAN1_IER.
      CAN1_ESR.
      CAN1_BTR.
      CAN1_TI0R.
      CAN1_TDT0R.
      CAN1_TDL0R.
      CAN1_TDH0R.
      CAN1_TI1R.
      CAN1_TDT1R.
      CAN1_TDL1R.
      CAN1_TDH1R.
      CAN1_TI2R.
      CAN1_TDT2R.
      CAN1_TDL2R.
      CAN1_TDH2R.
      CAN1_RI0R.
      CAN1_RDT0R.
      CAN1_RDL0R.
      CAN1_RDH0R.
      CAN1_RI1R.
      CAN1_RDT1R.
      CAN1_RDL1R.
      CAN1_RDH1R.
      CAN1_FMR.
      CAN1_FM1R.
      CAN1_FS1R.
      CAN1_FFA1R.
      CAN1_FA1R.
      CAN1_F0R1.
      CAN1_F0R2.
      CAN1_F1R1.
      CAN1_F1R2.
      CAN1_F2R1.
      CAN1_F2R2.
      CAN1_F3R1.
      CAN1_F3R2.
      CAN1_F4R1.
      CAN1_F4R2.
      CAN1_F5R1.
      CAN1_F5R2.
      CAN1_F6R1.
      CAN1_F6R2.
      CAN1_F7R1.
      CAN1_F7R2.
      CAN1_F8R1.
      CAN1_F8R2.
      CAN1_F9R1.
      CAN1_F9R2.
      CAN1_F10R1.
      CAN1_F10R2.
      CAN1_F11R1.
      CAN1_F11R2.
      CAN1_F12R1.
      CAN1_F12R2.
      CAN1_F13R1.
      CAN1_F13R2.
      CAN1_F14R1.
      CAN1_F14R2.
      CAN1_F15R1.
      CAN1_F15R2.
      CAN1_F16R1.
      CAN1_F16R2.
      CAN1_F17R1.
      CAN1_F17R2.
      CAN1_F18R1.
      CAN1_F18R2.
      CAN1_F19R1.
      CAN1_F19R2.
      CAN1_F20R1.
      CAN1_F20R2.
      CAN1_F21R1.
      CAN1_F21R2.
      CAN1_F22R1.
      CAN1_F22R2.
      CAN1_F23R1.
      CAN1_F23R2.
      CAN1_F24R1.
      CAN1_F24R2.
      CAN1_F25R1.
      CAN1_F25R2.
      CAN1_F26R1.
      CAN1_F26R2.
      CAN1_F27R1.
      CAN1_F27R2.
    ;
  [then]

  execute-defined? use-CAN2 [if]
    $40006800 constant CAN2 ( Controller area network ) 
    CAN2 $0 + constant CAN2_MCR ( read-write )  \ master control register
    CAN2 $4 + constant CAN2_MSR (  )  \ master status register
    CAN2 $8 + constant CAN2_TSR (  )  \ transmit status register
    CAN2 $C + constant CAN2_RF0R (  )  \ receive FIFO 0 register
    CAN2 $10 + constant CAN2_RF1R (  )  \ receive FIFO 1 register
    CAN2 $14 + constant CAN2_IER ( read-write )  \ interrupt enable register
    CAN2 $18 + constant CAN2_ESR (  )  \ interrupt enable register
    CAN2 $1C + constant CAN2_BTR ( read-write )  \ bit timing register
    CAN2 $180 + constant CAN2_TI0R ( read-write )  \ TX mailbox identifier register
    CAN2 $184 + constant CAN2_TDT0R ( read-write )  \ mailbox data length control and time stamp register
    CAN2 $188 + constant CAN2_TDL0R ( read-write )  \ mailbox data low register
    CAN2 $18C + constant CAN2_TDH0R ( read-write )  \ mailbox data high register
    CAN2 $190 + constant CAN2_TI1R ( read-write )  \ mailbox identifier register
    CAN2 $194 + constant CAN2_TDT1R ( read-write )  \ mailbox data length control and time stamp register
    CAN2 $198 + constant CAN2_TDL1R ( read-write )  \ mailbox data low register
    CAN2 $19C + constant CAN2_TDH1R ( read-write )  \ mailbox data high register
    CAN2 $1A0 + constant CAN2_TI2R ( read-write )  \ mailbox identifier register
    CAN2 $1A4 + constant CAN2_TDT2R ( read-write )  \ mailbox data length control and time stamp register
    CAN2 $1A8 + constant CAN2_TDL2R ( read-write )  \ mailbox data low register
    CAN2 $1AC + constant CAN2_TDH2R ( read-write )  \ mailbox data high register
    CAN2 $1B0 + constant CAN2_RI0R ( read-only )  \ receive FIFO mailbox identifier register
    CAN2 $1B4 + constant CAN2_RDT0R ( read-only )  \ mailbox data high register
    CAN2 $1B8 + constant CAN2_RDL0R ( read-only )  \ mailbox data high register
    CAN2 $1BC + constant CAN2_RDH0R ( read-only )  \ receive FIFO mailbox data high register
    CAN2 $1C0 + constant CAN2_RI1R ( read-only )  \ mailbox data high register
    CAN2 $1C4 + constant CAN2_RDT1R ( read-only )  \ mailbox data high register
    CAN2 $1C8 + constant CAN2_RDL1R ( read-only )  \ mailbox data high register
    CAN2 $1CC + constant CAN2_RDH1R ( read-only )  \ mailbox data high register
    CAN2 $200 + constant CAN2_FMR ( read-write )  \ filter master register
    CAN2 $204 + constant CAN2_FM1R ( read-write )  \ filter mode register
    CAN2 $20C + constant CAN2_FS1R ( read-write )  \ filter scale register
    CAN2 $214 + constant CAN2_FFA1R ( read-write )  \ filter FIFO assignment register
    CAN2 $21C + constant CAN2_FA1R ( read-write )  \ filter activation register
    CAN2 $240 + constant CAN2_F0R1 ( read-write )  \ Filter bank 0 register 1
    CAN2 $244 + constant CAN2_F0R2 ( read-write )  \ Filter bank 0 register 2
    CAN2 $248 + constant CAN2_F1R1 ( read-write )  \ Filter bank 1 register 1
    CAN2 $24C + constant CAN2_F1R2 ( read-write )  \ Filter bank 1 register 2
    CAN2 $250 + constant CAN2_F2R1 ( read-write )  \ Filter bank 2 register 1
    CAN2 $254 + constant CAN2_F2R2 ( read-write )  \ Filter bank 2 register 2
    CAN2 $258 + constant CAN2_F3R1 ( read-write )  \ Filter bank 3 register 1
    CAN2 $25C + constant CAN2_F3R2 ( read-write )  \ Filter bank 3 register 2
    CAN2 $260 + constant CAN2_F4R1 ( read-write )  \ Filter bank 4 register 1
    CAN2 $264 + constant CAN2_F4R2 ( read-write )  \ Filter bank 4 register 2
    CAN2 $268 + constant CAN2_F5R1 ( read-write )  \ Filter bank 5 register 1
    CAN2 $26C + constant CAN2_F5R2 ( read-write )  \ Filter bank 5 register 2
    CAN2 $270 + constant CAN2_F6R1 ( read-write )  \ Filter bank 6 register 1
    CAN2 $274 + constant CAN2_F6R2 ( read-write )  \ Filter bank 6 register 2
    CAN2 $278 + constant CAN2_F7R1 ( read-write )  \ Filter bank 7 register 1
    CAN2 $27C + constant CAN2_F7R2 ( read-write )  \ Filter bank 7 register 2
    CAN2 $280 + constant CAN2_F8R1 ( read-write )  \ Filter bank 8 register 1
    CAN2 $284 + constant CAN2_F8R2 ( read-write )  \ Filter bank 8 register 2
    CAN2 $288 + constant CAN2_F9R1 ( read-write )  \ Filter bank 9 register 1
    CAN2 $28C + constant CAN2_F9R2 ( read-write )  \ Filter bank 9 register 2
    CAN2 $290 + constant CAN2_F10R1 ( read-write )  \ Filter bank 10 register 1
    CAN2 $294 + constant CAN2_F10R2 ( read-write )  \ Filter bank 10 register 2
    CAN2 $298 + constant CAN2_F11R1 ( read-write )  \ Filter bank 11 register 1
    CAN2 $29C + constant CAN2_F11R2 ( read-write )  \ Filter bank 11 register 2
    CAN2 $2A0 + constant CAN2_F12R1 ( read-write )  \ Filter bank 4 register 1
    CAN2 $2A4 + constant CAN2_F12R2 ( read-write )  \ Filter bank 12 register 2
    CAN2 $2A8 + constant CAN2_F13R1 ( read-write )  \ Filter bank 13 register 1
    CAN2 $2AC + constant CAN2_F13R2 ( read-write )  \ Filter bank 13 register 2
    CAN2 $2B0 + constant CAN2_F14R1 ( read-write )  \ Filter bank 14 register 1
    CAN2 $2B4 + constant CAN2_F14R2 ( read-write )  \ Filter bank 14 register 2
    CAN2 $2B8 + constant CAN2_F15R1 ( read-write )  \ Filter bank 15 register 1
    CAN2 $2BC + constant CAN2_F15R2 ( read-write )  \ Filter bank 15 register 2
    CAN2 $2C0 + constant CAN2_F16R1 ( read-write )  \ Filter bank 16 register 1
    CAN2 $2C4 + constant CAN2_F16R2 ( read-write )  \ Filter bank 16 register 2
    CAN2 $2C8 + constant CAN2_F17R1 ( read-write )  \ Filter bank 17 register 1
    CAN2 $2CC + constant CAN2_F17R2 ( read-write )  \ Filter bank 17 register 2
    CAN2 $2D0 + constant CAN2_F18R1 ( read-write )  \ Filter bank 18 register 1
    CAN2 $2D4 + constant CAN2_F18R2 ( read-write )  \ Filter bank 18 register 2
    CAN2 $2D8 + constant CAN2_F19R1 ( read-write )  \ Filter bank 19 register 1
    CAN2 $2DC + constant CAN2_F19R2 ( read-write )  \ Filter bank 19 register 2
    CAN2 $2E0 + constant CAN2_F20R1 ( read-write )  \ Filter bank 20 register 1
    CAN2 $2E4 + constant CAN2_F20R2 ( read-write )  \ Filter bank 20 register 2
    CAN2 $2E8 + constant CAN2_F21R1 ( read-write )  \ Filter bank 21 register 1
    CAN2 $2EC + constant CAN2_F21R2 ( read-write )  \ Filter bank 21 register 2
    CAN2 $2F0 + constant CAN2_F22R1 ( read-write )  \ Filter bank 22 register 1
    CAN2 $2F4 + constant CAN2_F22R2 ( read-write )  \ Filter bank 22 register 2
    CAN2 $2F8 + constant CAN2_F23R1 ( read-write )  \ Filter bank 23 register 1
    CAN2 $2FC + constant CAN2_F23R2 ( read-write )  \ Filter bank 23 register 2
    CAN2 $300 + constant CAN2_F24R1 ( read-write )  \ Filter bank 24 register 1
    CAN2 $304 + constant CAN2_F24R2 ( read-write )  \ Filter bank 24 register 2
    CAN2 $308 + constant CAN2_F25R1 ( read-write )  \ Filter bank 25 register 1
    CAN2 $30C + constant CAN2_F25R2 ( read-write )  \ Filter bank 25 register 2
    CAN2 $310 + constant CAN2_F26R1 ( read-write )  \ Filter bank 26 register 1
    CAN2 $314 + constant CAN2_F26R2 ( read-write )  \ Filter bank 26 register 2
    CAN2 $318 + constant CAN2_F27R1 ( read-write )  \ Filter bank 27 register 1
    CAN2 $31C + constant CAN2_F27R2 ( read-write )  \ Filter bank 27 register 2
    : CAN2_MCR. cr ." CAN2_MCR.  RW   $" CAN2_MCR @ hex. CAN2_MCR 1b. ;
    : CAN2_MSR. cr ." CAN2_MSR.   $" CAN2_MSR @ hex. CAN2_MSR 1b. ;
    : CAN2_TSR. cr ." CAN2_TSR.   $" CAN2_TSR @ hex. CAN2_TSR 1b. ;
    : CAN2_RF0R. cr ." CAN2_RF0R.   $" CAN2_RF0R @ hex. CAN2_RF0R 1b. ;
    : CAN2_RF1R. cr ." CAN2_RF1R.   $" CAN2_RF1R @ hex. CAN2_RF1R 1b. ;
    : CAN2_IER. cr ." CAN2_IER.  RW   $" CAN2_IER @ hex. CAN2_IER 1b. ;
    : CAN2_ESR. cr ." CAN2_ESR.   $" CAN2_ESR @ hex. CAN2_ESR 1b. ;
    : CAN2_BTR. cr ." CAN2_BTR.  RW   $" CAN2_BTR @ hex. CAN2_BTR 1b. ;
    : CAN2_TI0R. cr ." CAN2_TI0R.  RW   $" CAN2_TI0R @ hex. CAN2_TI0R 1b. ;
    : CAN2_TDT0R. cr ." CAN2_TDT0R.  RW   $" CAN2_TDT0R @ hex. CAN2_TDT0R 1b. ;
    : CAN2_TDL0R. cr ." CAN2_TDL0R.  RW   $" CAN2_TDL0R @ hex. CAN2_TDL0R 1b. ;
    : CAN2_TDH0R. cr ." CAN2_TDH0R.  RW   $" CAN2_TDH0R @ hex. CAN2_TDH0R 1b. ;
    : CAN2_TI1R. cr ." CAN2_TI1R.  RW   $" CAN2_TI1R @ hex. CAN2_TI1R 1b. ;
    : CAN2_TDT1R. cr ." CAN2_TDT1R.  RW   $" CAN2_TDT1R @ hex. CAN2_TDT1R 1b. ;
    : CAN2_TDL1R. cr ." CAN2_TDL1R.  RW   $" CAN2_TDL1R @ hex. CAN2_TDL1R 1b. ;
    : CAN2_TDH1R. cr ." CAN2_TDH1R.  RW   $" CAN2_TDH1R @ hex. CAN2_TDH1R 1b. ;
    : CAN2_TI2R. cr ." CAN2_TI2R.  RW   $" CAN2_TI2R @ hex. CAN2_TI2R 1b. ;
    : CAN2_TDT2R. cr ." CAN2_TDT2R.  RW   $" CAN2_TDT2R @ hex. CAN2_TDT2R 1b. ;
    : CAN2_TDL2R. cr ." CAN2_TDL2R.  RW   $" CAN2_TDL2R @ hex. CAN2_TDL2R 1b. ;
    : CAN2_TDH2R. cr ." CAN2_TDH2R.  RW   $" CAN2_TDH2R @ hex. CAN2_TDH2R 1b. ;
    : CAN2_RI0R. cr ." CAN2_RI0R.  RO   $" CAN2_RI0R @ hex. CAN2_RI0R 1b. ;
    : CAN2_RDT0R. cr ." CAN2_RDT0R.  RO   $" CAN2_RDT0R @ hex. CAN2_RDT0R 1b. ;
    : CAN2_RDL0R. cr ." CAN2_RDL0R.  RO   $" CAN2_RDL0R @ hex. CAN2_RDL0R 1b. ;
    : CAN2_RDH0R. cr ." CAN2_RDH0R.  RO   $" CAN2_RDH0R @ hex. CAN2_RDH0R 1b. ;
    : CAN2_RI1R. cr ." CAN2_RI1R.  RO   $" CAN2_RI1R @ hex. CAN2_RI1R 1b. ;
    : CAN2_RDT1R. cr ." CAN2_RDT1R.  RO   $" CAN2_RDT1R @ hex. CAN2_RDT1R 1b. ;
    : CAN2_RDL1R. cr ." CAN2_RDL1R.  RO   $" CAN2_RDL1R @ hex. CAN2_RDL1R 1b. ;
    : CAN2_RDH1R. cr ." CAN2_RDH1R.  RO   $" CAN2_RDH1R @ hex. CAN2_RDH1R 1b. ;
    : CAN2_FMR. cr ." CAN2_FMR.  RW   $" CAN2_FMR @ hex. CAN2_FMR 1b. ;
    : CAN2_FM1R. cr ." CAN2_FM1R.  RW   $" CAN2_FM1R @ hex. CAN2_FM1R 1b. ;
    : CAN2_FS1R. cr ." CAN2_FS1R.  RW   $" CAN2_FS1R @ hex. CAN2_FS1R 1b. ;
    : CAN2_FFA1R. cr ." CAN2_FFA1R.  RW   $" CAN2_FFA1R @ hex. CAN2_FFA1R 1b. ;
    : CAN2_FA1R. cr ." CAN2_FA1R.  RW   $" CAN2_FA1R @ hex. CAN2_FA1R 1b. ;
    : CAN2_F0R1. cr ." CAN2_F0R1.  RW   $" CAN2_F0R1 @ hex. CAN2_F0R1 1b. ;
    : CAN2_F0R2. cr ." CAN2_F0R2.  RW   $" CAN2_F0R2 @ hex. CAN2_F0R2 1b. ;
    : CAN2_F1R1. cr ." CAN2_F1R1.  RW   $" CAN2_F1R1 @ hex. CAN2_F1R1 1b. ;
    : CAN2_F1R2. cr ." CAN2_F1R2.  RW   $" CAN2_F1R2 @ hex. CAN2_F1R2 1b. ;
    : CAN2_F2R1. cr ." CAN2_F2R1.  RW   $" CAN2_F2R1 @ hex. CAN2_F2R1 1b. ;
    : CAN2_F2R2. cr ." CAN2_F2R2.  RW   $" CAN2_F2R2 @ hex. CAN2_F2R2 1b. ;
    : CAN2_F3R1. cr ." CAN2_F3R1.  RW   $" CAN2_F3R1 @ hex. CAN2_F3R1 1b. ;
    : CAN2_F3R2. cr ." CAN2_F3R2.  RW   $" CAN2_F3R2 @ hex. CAN2_F3R2 1b. ;
    : CAN2_F4R1. cr ." CAN2_F4R1.  RW   $" CAN2_F4R1 @ hex. CAN2_F4R1 1b. ;
    : CAN2_F4R2. cr ." CAN2_F4R2.  RW   $" CAN2_F4R2 @ hex. CAN2_F4R2 1b. ;
    : CAN2_F5R1. cr ." CAN2_F5R1.  RW   $" CAN2_F5R1 @ hex. CAN2_F5R1 1b. ;
    : CAN2_F5R2. cr ." CAN2_F5R2.  RW   $" CAN2_F5R2 @ hex. CAN2_F5R2 1b. ;
    : CAN2_F6R1. cr ." CAN2_F6R1.  RW   $" CAN2_F6R1 @ hex. CAN2_F6R1 1b. ;
    : CAN2_F6R2. cr ." CAN2_F6R2.  RW   $" CAN2_F6R2 @ hex. CAN2_F6R2 1b. ;
    : CAN2_F7R1. cr ." CAN2_F7R1.  RW   $" CAN2_F7R1 @ hex. CAN2_F7R1 1b. ;
    : CAN2_F7R2. cr ." CAN2_F7R2.  RW   $" CAN2_F7R2 @ hex. CAN2_F7R2 1b. ;
    : CAN2_F8R1. cr ." CAN2_F8R1.  RW   $" CAN2_F8R1 @ hex. CAN2_F8R1 1b. ;
    : CAN2_F8R2. cr ." CAN2_F8R2.  RW   $" CAN2_F8R2 @ hex. CAN2_F8R2 1b. ;
    : CAN2_F9R1. cr ." CAN2_F9R1.  RW   $" CAN2_F9R1 @ hex. CAN2_F9R1 1b. ;
    : CAN2_F9R2. cr ." CAN2_F9R2.  RW   $" CAN2_F9R2 @ hex. CAN2_F9R2 1b. ;
    : CAN2_F10R1. cr ." CAN2_F10R1.  RW   $" CAN2_F10R1 @ hex. CAN2_F10R1 1b. ;
    : CAN2_F10R2. cr ." CAN2_F10R2.  RW   $" CAN2_F10R2 @ hex. CAN2_F10R2 1b. ;
    : CAN2_F11R1. cr ." CAN2_F11R1.  RW   $" CAN2_F11R1 @ hex. CAN2_F11R1 1b. ;
    : CAN2_F11R2. cr ." CAN2_F11R2.  RW   $" CAN2_F11R2 @ hex. CAN2_F11R2 1b. ;
    : CAN2_F12R1. cr ." CAN2_F12R1.  RW   $" CAN2_F12R1 @ hex. CAN2_F12R1 1b. ;
    : CAN2_F12R2. cr ." CAN2_F12R2.  RW   $" CAN2_F12R2 @ hex. CAN2_F12R2 1b. ;
    : CAN2_F13R1. cr ." CAN2_F13R1.  RW   $" CAN2_F13R1 @ hex. CAN2_F13R1 1b. ;
    : CAN2_F13R2. cr ." CAN2_F13R2.  RW   $" CAN2_F13R2 @ hex. CAN2_F13R2 1b. ;
    : CAN2_F14R1. cr ." CAN2_F14R1.  RW   $" CAN2_F14R1 @ hex. CAN2_F14R1 1b. ;
    : CAN2_F14R2. cr ." CAN2_F14R2.  RW   $" CAN2_F14R2 @ hex. CAN2_F14R2 1b. ;
    : CAN2_F15R1. cr ." CAN2_F15R1.  RW   $" CAN2_F15R1 @ hex. CAN2_F15R1 1b. ;
    : CAN2_F15R2. cr ." CAN2_F15R2.  RW   $" CAN2_F15R2 @ hex. CAN2_F15R2 1b. ;
    : CAN2_F16R1. cr ." CAN2_F16R1.  RW   $" CAN2_F16R1 @ hex. CAN2_F16R1 1b. ;
    : CAN2_F16R2. cr ." CAN2_F16R2.  RW   $" CAN2_F16R2 @ hex. CAN2_F16R2 1b. ;
    : CAN2_F17R1. cr ." CAN2_F17R1.  RW   $" CAN2_F17R1 @ hex. CAN2_F17R1 1b. ;
    : CAN2_F17R2. cr ." CAN2_F17R2.  RW   $" CAN2_F17R2 @ hex. CAN2_F17R2 1b. ;
    : CAN2_F18R1. cr ." CAN2_F18R1.  RW   $" CAN2_F18R1 @ hex. CAN2_F18R1 1b. ;
    : CAN2_F18R2. cr ." CAN2_F18R2.  RW   $" CAN2_F18R2 @ hex. CAN2_F18R2 1b. ;
    : CAN2_F19R1. cr ." CAN2_F19R1.  RW   $" CAN2_F19R1 @ hex. CAN2_F19R1 1b. ;
    : CAN2_F19R2. cr ." CAN2_F19R2.  RW   $" CAN2_F19R2 @ hex. CAN2_F19R2 1b. ;
    : CAN2_F20R1. cr ." CAN2_F20R1.  RW   $" CAN2_F20R1 @ hex. CAN2_F20R1 1b. ;
    : CAN2_F20R2. cr ." CAN2_F20R2.  RW   $" CAN2_F20R2 @ hex. CAN2_F20R2 1b. ;
    : CAN2_F21R1. cr ." CAN2_F21R1.  RW   $" CAN2_F21R1 @ hex. CAN2_F21R1 1b. ;
    : CAN2_F21R2. cr ." CAN2_F21R2.  RW   $" CAN2_F21R2 @ hex. CAN2_F21R2 1b. ;
    : CAN2_F22R1. cr ." CAN2_F22R1.  RW   $" CAN2_F22R1 @ hex. CAN2_F22R1 1b. ;
    : CAN2_F22R2. cr ." CAN2_F22R2.  RW   $" CAN2_F22R2 @ hex. CAN2_F22R2 1b. ;
    : CAN2_F23R1. cr ." CAN2_F23R1.  RW   $" CAN2_F23R1 @ hex. CAN2_F23R1 1b. ;
    : CAN2_F23R2. cr ." CAN2_F23R2.  RW   $" CAN2_F23R2 @ hex. CAN2_F23R2 1b. ;
    : CAN2_F24R1. cr ." CAN2_F24R1.  RW   $" CAN2_F24R1 @ hex. CAN2_F24R1 1b. ;
    : CAN2_F24R2. cr ." CAN2_F24R2.  RW   $" CAN2_F24R2 @ hex. CAN2_F24R2 1b. ;
    : CAN2_F25R1. cr ." CAN2_F25R1.  RW   $" CAN2_F25R1 @ hex. CAN2_F25R1 1b. ;
    : CAN2_F25R2. cr ." CAN2_F25R2.  RW   $" CAN2_F25R2 @ hex. CAN2_F25R2 1b. ;
    : CAN2_F26R1. cr ." CAN2_F26R1.  RW   $" CAN2_F26R1 @ hex. CAN2_F26R1 1b. ;
    : CAN2_F26R2. cr ." CAN2_F26R2.  RW   $" CAN2_F26R2 @ hex. CAN2_F26R2 1b. ;
    : CAN2_F27R1. cr ." CAN2_F27R1.  RW   $" CAN2_F27R1 @ hex. CAN2_F27R1 1b. ;
    : CAN2_F27R2. cr ." CAN2_F27R2.  RW   $" CAN2_F27R2 @ hex. CAN2_F27R2 1b. ;
    : CAN2.
      CAN2_MCR.
      CAN2_MSR.
      CAN2_TSR.
      CAN2_RF0R.
      CAN2_RF1R.
      CAN2_IER.
      CAN2_ESR.
      CAN2_BTR.
      CAN2_TI0R.
      CAN2_TDT0R.
      CAN2_TDL0R.
      CAN2_TDH0R.
      CAN2_TI1R.
      CAN2_TDT1R.
      CAN2_TDL1R.
      CAN2_TDH1R.
      CAN2_TI2R.
      CAN2_TDT2R.
      CAN2_TDL2R.
      CAN2_TDH2R.
      CAN2_RI0R.
      CAN2_RDT0R.
      CAN2_RDL0R.
      CAN2_RDH0R.
      CAN2_RI1R.
      CAN2_RDT1R.
      CAN2_RDL1R.
      CAN2_RDH1R.
      CAN2_FMR.
      CAN2_FM1R.
      CAN2_FS1R.
      CAN2_FFA1R.
      CAN2_FA1R.
      CAN2_F0R1.
      CAN2_F0R2.
      CAN2_F1R1.
      CAN2_F1R2.
      CAN2_F2R1.
      CAN2_F2R2.
      CAN2_F3R1.
      CAN2_F3R2.
      CAN2_F4R1.
      CAN2_F4R2.
      CAN2_F5R1.
      CAN2_F5R2.
      CAN2_F6R1.
      CAN2_F6R2.
      CAN2_F7R1.
      CAN2_F7R2.
      CAN2_F8R1.
      CAN2_F8R2.
      CAN2_F9R1.
      CAN2_F9R2.
      CAN2_F10R1.
      CAN2_F10R2.
      CAN2_F11R1.
      CAN2_F11R2.
      CAN2_F12R1.
      CAN2_F12R2.
      CAN2_F13R1.
      CAN2_F13R2.
      CAN2_F14R1.
      CAN2_F14R2.
      CAN2_F15R1.
      CAN2_F15R2.
      CAN2_F16R1.
      CAN2_F16R2.
      CAN2_F17R1.
      CAN2_F17R2.
      CAN2_F18R1.
      CAN2_F18R2.
      CAN2_F19R1.
      CAN2_F19R2.
      CAN2_F20R1.
      CAN2_F20R2.
      CAN2_F21R1.
      CAN2_F21R2.
      CAN2_F22R1.
      CAN2_F22R2.
      CAN2_F23R1.
      CAN2_F23R2.
      CAN2_F24R1.
      CAN2_F24R2.
      CAN2_F25R1.
      CAN2_F25R2.
      CAN2_F26R1.
      CAN2_F26R2.
      CAN2_F27R1.
      CAN2_F27R2.
    ;
  [then]

  execute-defined? use-FLASH [if]
    $40023C00 constant FLASH ( FLASH ) 
    FLASH $0 + constant FLASH_ACR ( read-write )  \ Flash access control register
    FLASH $4 + constant FLASH_KEYR ( write-only )  \ Flash key register
    FLASH $8 + constant FLASH_OPTKEYR ( write-only )  \ Flash option key register
    FLASH $C + constant FLASH_SR (  )  \ Status register
    FLASH $10 + constant FLASH_CR ( read-write )  \ Control register
    FLASH $14 + constant FLASH_OPTCR ( read-write )  \ Flash option control register
    FLASH $18 + constant FLASH_OPTCR1 ( read-write )  \ Flash option control register 1
    : FLASH_ACR. cr ." FLASH_ACR.  RW   $" FLASH_ACR @ hex. FLASH_ACR 1b. ;
    : FLASH_KEYR. cr ." FLASH_KEYR " WRITEONLY ; 
    : FLASH_OPTKEYR. cr ." FLASH_OPTKEYR " WRITEONLY ; 
    : FLASH_SR. cr ." FLASH_SR.   $" FLASH_SR @ hex. FLASH_SR 1b. ;
    : FLASH_CR. cr ." FLASH_CR.  RW   $" FLASH_CR @ hex. FLASH_CR 1b. ;
    : FLASH_OPTCR. cr ." FLASH_OPTCR.  RW   $" FLASH_OPTCR @ hex. FLASH_OPTCR 1b. ;
    : FLASH_OPTCR1. cr ." FLASH_OPTCR1.  RW   $" FLASH_OPTCR1 @ hex. FLASH_OPTCR1 1b. ;
    : FLASH.
      FLASH_ACR.
      FLASH_KEYR.
      FLASH_OPTKEYR.
      FLASH_SR.
      FLASH_CR.
      FLASH_OPTCR.
      FLASH_OPTCR1.
    ;
  [then]

  execute-defined? use-EXTI [if]
    $40013C00 constant EXTI ( External interrupt/event controller ) 
    EXTI $0 + constant EXTI_IMR ( read-write )  \ Interrupt mask register EXTI_IMR
    EXTI $4 + constant EXTI_EMR ( read-write )  \ Event mask register EXTI_EMR
    EXTI $8 + constant EXTI_RTSR ( read-write )  \ Rising Trigger selection register EXTI_RTSR
    EXTI $C + constant EXTI_FTSR ( read-write )  \ Falling Trigger selection register EXTI_FTSR
    EXTI $10 + constant EXTI_SWIER ( read-write )  \ Software interrupt event register EXTI_SWIER
    EXTI $14 + constant EXTI_PR ( read-write )  \ Pending register EXTI_PR
    : EXTI_IMR. cr ." EXTI_IMR.  RW   $" EXTI_IMR @ hex. EXTI_IMR 1b. ;
    : EXTI_EMR. cr ." EXTI_EMR.  RW   $" EXTI_EMR @ hex. EXTI_EMR 1b. ;
    : EXTI_RTSR. cr ." EXTI_RTSR.  RW   $" EXTI_RTSR @ hex. EXTI_RTSR 1b. ;
    : EXTI_FTSR. cr ." EXTI_FTSR.  RW   $" EXTI_FTSR @ hex. EXTI_FTSR 1b. ;
    : EXTI_SWIER. cr ." EXTI_SWIER.  RW   $" EXTI_SWIER @ hex. EXTI_SWIER 1b. ;
    : EXTI_PR. cr ." EXTI_PR.  RW   $" EXTI_PR @ hex. EXTI_PR 1b. ;
    : EXTI.
      EXTI_IMR.
      EXTI_EMR.
      EXTI_RTSR.
      EXTI_FTSR.
      EXTI_SWIER.
      EXTI_PR.
    ;
  [then]

  execute-defined? use-LTDC [if]
    $40016800 constant LTDC ( LCD-TFT Controller ) 
    LTDC $8 + constant LTDC_SSCR ( read-write )  \ Synchronization Size Configuration Register
    LTDC $C + constant LTDC_BPCR ( read-write )  \ Back Porch Configuration Register
    LTDC $10 + constant LTDC_AWCR ( read-write )  \ Active Width Configuration Register
    LTDC $14 + constant LTDC_TWCR ( read-write )  \ Total Width Configuration Register
    LTDC $18 + constant LTDC_GCR (  )  \ Global Control Register
    LTDC $24 + constant LTDC_SRCR ( read-write )  \ Shadow Reload Configuration Register
    LTDC $2C + constant LTDC_BCCR ( read-write )  \ Background Color Configuration Register
    LTDC $34 + constant LTDC_IER ( read-write )  \ Interrupt Enable Register
    LTDC $38 + constant LTDC_ISR ( read-only )  \ Interrupt Status Register
    LTDC $3C + constant LTDC_ICR ( write-only )  \ Interrupt Clear Register
    LTDC $40 + constant LTDC_LIPCR ( read-write )  \ Line Interrupt Position Configuration Register
    LTDC $44 + constant LTDC_CPSR ( read-only )  \ Current Position Status Register
    LTDC $48 + constant LTDC_CDSR ( read-only )  \ Current Display Status Register
    LTDC $84 + constant LTDC_L1CR ( read-write )  \ Layerx Control Register
    LTDC $88 + constant LTDC_L1WHPCR ( read-write )  \ Layerx Window Horizontal Position Configuration Register
    LTDC $8C + constant LTDC_L1WVPCR ( read-write )  \ Layerx Window Vertical Position Configuration Register
    LTDC $90 + constant LTDC_L1CKCR ( read-write )  \ Layerx Color Keying Configuration Register
    LTDC $94 + constant LTDC_L1PFCR ( read-write )  \ Layerx Pixel Format Configuration Register
    LTDC $98 + constant LTDC_L1CACR ( read-write )  \ Layerx Constant Alpha Configuration Register
    LTDC $9C + constant LTDC_L1DCCR ( read-write )  \ Layerx Default Color Configuration Register
    LTDC $A0 + constant LTDC_L1BFCR ( read-write )  \ Layerx Blending Factors Configuration Register
    LTDC $AC + constant LTDC_L1CFBAR ( read-write )  \ Layerx Color Frame Buffer Address Register
    LTDC $B0 + constant LTDC_L1CFBLR ( read-write )  \ Layerx Color Frame Buffer Length Register
    LTDC $B4 + constant LTDC_L1CFBLNR ( read-write )  \ Layerx ColorFrame Buffer Line Number Register
    LTDC $C4 + constant LTDC_L1CLUTWR ( write-only )  \ Layerx CLUT Write Register
    LTDC $104 + constant LTDC_L2CR ( read-write )  \ Layerx Control Register
    LTDC $108 + constant LTDC_L2WHPCR ( read-write )  \ Layerx Window Horizontal Position Configuration Register
    LTDC $10C + constant LTDC_L2WVPCR ( read-write )  \ Layerx Window Vertical Position Configuration Register
    LTDC $110 + constant LTDC_L2CKCR ( read-write )  \ Layerx Color Keying Configuration Register
    LTDC $114 + constant LTDC_L2PFCR ( read-write )  \ Layerx Pixel Format Configuration Register
    LTDC $118 + constant LTDC_L2CACR ( read-write )  \ Layerx Constant Alpha Configuration Register
    LTDC $11C + constant LTDC_L2DCCR ( read-write )  \ Layerx Default Color Configuration Register
    LTDC $120 + constant LTDC_L2BFCR ( read-write )  \ Layerx Blending Factors Configuration Register
    LTDC $12C + constant LTDC_L2CFBAR ( read-write )  \ Layerx Color Frame Buffer Address Register
    LTDC $130 + constant LTDC_L2CFBLR ( read-write )  \ Layerx Color Frame Buffer Length Register
    LTDC $134 + constant LTDC_L2CFBLNR ( read-write )  \ Layerx ColorFrame Buffer Line Number Register
    LTDC $144 + constant LTDC_L2CLUTWR ( write-only )  \ Layerx CLUT Write Register
    : LTDC_SSCR. cr ." LTDC_SSCR.  RW   $" LTDC_SSCR @ hex. LTDC_SSCR 1b. ;
    : LTDC_BPCR. cr ." LTDC_BPCR.  RW   $" LTDC_BPCR @ hex. LTDC_BPCR 1b. ;
    : LTDC_AWCR. cr ." LTDC_AWCR.  RW   $" LTDC_AWCR @ hex. LTDC_AWCR 1b. ;
    : LTDC_TWCR. cr ." LTDC_TWCR.  RW   $" LTDC_TWCR @ hex. LTDC_TWCR 1b. ;
    : LTDC_GCR. cr ." LTDC_GCR.   $" LTDC_GCR @ hex. LTDC_GCR 1b. ;
    : LTDC_SRCR. cr ." LTDC_SRCR.  RW   $" LTDC_SRCR @ hex. LTDC_SRCR 1b. ;
    : LTDC_BCCR. cr ." LTDC_BCCR.  RW   $" LTDC_BCCR @ hex. LTDC_BCCR 1b. ;
    : LTDC_IER. cr ." LTDC_IER.  RW   $" LTDC_IER @ hex. LTDC_IER 1b. ;
    : LTDC_ISR. cr ." LTDC_ISR.  RO   $" LTDC_ISR @ hex. LTDC_ISR 1b. ;
    : LTDC_ICR. cr ." LTDC_ICR " WRITEONLY ; 
    : LTDC_LIPCR. cr ." LTDC_LIPCR.  RW   $" LTDC_LIPCR @ hex. LTDC_LIPCR 1b. ;
    : LTDC_CPSR. cr ." LTDC_CPSR.  RO   $" LTDC_CPSR @ hex. LTDC_CPSR 1b. ;
    : LTDC_CDSR. cr ." LTDC_CDSR.  RO   $" LTDC_CDSR @ hex. LTDC_CDSR 1b. ;
    : LTDC_L1CR. cr ." LTDC_L1CR.  RW   $" LTDC_L1CR @ hex. LTDC_L1CR 1b. ;
    : LTDC_L1WHPCR. cr ." LTDC_L1WHPCR.  RW   $" LTDC_L1WHPCR @ hex. LTDC_L1WHPCR 1b. ;
    : LTDC_L1WVPCR. cr ." LTDC_L1WVPCR.  RW   $" LTDC_L1WVPCR @ hex. LTDC_L1WVPCR 1b. ;
    : LTDC_L1CKCR. cr ." LTDC_L1CKCR.  RW   $" LTDC_L1CKCR @ hex. LTDC_L1CKCR 1b. ;
    : LTDC_L1PFCR. cr ." LTDC_L1PFCR.  RW   $" LTDC_L1PFCR @ hex. LTDC_L1PFCR 1b. ;
    : LTDC_L1CACR. cr ." LTDC_L1CACR.  RW   $" LTDC_L1CACR @ hex. LTDC_L1CACR 1b. ;
    : LTDC_L1DCCR. cr ." LTDC_L1DCCR.  RW   $" LTDC_L1DCCR @ hex. LTDC_L1DCCR 1b. ;
    : LTDC_L1BFCR. cr ." LTDC_L1BFCR.  RW   $" LTDC_L1BFCR @ hex. LTDC_L1BFCR 1b. ;
    : LTDC_L1CFBAR. cr ." LTDC_L1CFBAR.  RW   $" LTDC_L1CFBAR @ hex. LTDC_L1CFBAR 1b. ;
    : LTDC_L1CFBLR. cr ." LTDC_L1CFBLR.  RW   $" LTDC_L1CFBLR @ hex. LTDC_L1CFBLR 1b. ;
    : LTDC_L1CFBLNR. cr ." LTDC_L1CFBLNR.  RW   $" LTDC_L1CFBLNR @ hex. LTDC_L1CFBLNR 1b. ;
    : LTDC_L1CLUTWR. cr ." LTDC_L1CLUTWR " WRITEONLY ; 
    : LTDC_L2CR. cr ." LTDC_L2CR.  RW   $" LTDC_L2CR @ hex. LTDC_L2CR 1b. ;
    : LTDC_L2WHPCR. cr ." LTDC_L2WHPCR.  RW   $" LTDC_L2WHPCR @ hex. LTDC_L2WHPCR 1b. ;
    : LTDC_L2WVPCR. cr ." LTDC_L2WVPCR.  RW   $" LTDC_L2WVPCR @ hex. LTDC_L2WVPCR 1b. ;
    : LTDC_L2CKCR. cr ." LTDC_L2CKCR.  RW   $" LTDC_L2CKCR @ hex. LTDC_L2CKCR 1b. ;
    : LTDC_L2PFCR. cr ." LTDC_L2PFCR.  RW   $" LTDC_L2PFCR @ hex. LTDC_L2PFCR 1b. ;
    : LTDC_L2CACR. cr ." LTDC_L2CACR.  RW   $" LTDC_L2CACR @ hex. LTDC_L2CACR 1b. ;
    : LTDC_L2DCCR. cr ." LTDC_L2DCCR.  RW   $" LTDC_L2DCCR @ hex. LTDC_L2DCCR 1b. ;
    : LTDC_L2BFCR. cr ." LTDC_L2BFCR.  RW   $" LTDC_L2BFCR @ hex. LTDC_L2BFCR 1b. ;
    : LTDC_L2CFBAR. cr ." LTDC_L2CFBAR.  RW   $" LTDC_L2CFBAR @ hex. LTDC_L2CFBAR 1b. ;
    : LTDC_L2CFBLR. cr ." LTDC_L2CFBLR.  RW   $" LTDC_L2CFBLR @ hex. LTDC_L2CFBLR 1b. ;
    : LTDC_L2CFBLNR. cr ." LTDC_L2CFBLNR.  RW   $" LTDC_L2CFBLNR @ hex. LTDC_L2CFBLNR 1b. ;
    : LTDC_L2CLUTWR. cr ." LTDC_L2CLUTWR " WRITEONLY ; 
    : LTDC.
      LTDC_SSCR.
      LTDC_BPCR.
      LTDC_AWCR.
      LTDC_TWCR.
      LTDC_GCR.
      LTDC_SRCR.
      LTDC_BCCR.
      LTDC_IER.
      LTDC_ISR.
      LTDC_ICR.
      LTDC_LIPCR.
      LTDC_CPSR.
      LTDC_CDSR.
      LTDC_L1CR.
      LTDC_L1WHPCR.
      LTDC_L1WVPCR.
      LTDC_L1CKCR.
      LTDC_L1PFCR.
      LTDC_L1CACR.
      LTDC_L1DCCR.
      LTDC_L1BFCR.
      LTDC_L1CFBAR.
      LTDC_L1CFBLR.
      LTDC_L1CFBLNR.
      LTDC_L1CLUTWR.
      LTDC_L2CR.
      LTDC_L2WHPCR.
      LTDC_L2WVPCR.
      LTDC_L2CKCR.
      LTDC_L2PFCR.
      LTDC_L2CACR.
      LTDC_L2DCCR.
      LTDC_L2BFCR.
      LTDC_L2CFBAR.
      LTDC_L2CFBLR.
      LTDC_L2CFBLNR.
      LTDC_L2CLUTWR.
    ;
  [then]

  execute-defined? use-SAI1 [if]
    $40015800 constant SAI1 ( Serial audio interface ) 
    SAI1 $24 + constant SAI1_BCR1 ( read-write )  \ BConfiguration register 1
    SAI1 $28 + constant SAI1_BCR2 ( read-write )  \ BConfiguration register 2
    SAI1 $2C + constant SAI1_BFRCR ( read-write )  \ BFRCR
    SAI1 $30 + constant SAI1_BSLOTR ( read-write )  \ BSlot register
    SAI1 $34 + constant SAI1_BIM ( read-write )  \ BInterrupt mask register2
    SAI1 $38 + constant SAI1_BSR ( read-only )  \ BStatus register
    SAI1 $3C + constant SAI1_BCLRFR ( write-only )  \ BClear flag register
    SAI1 $40 + constant SAI1_BDR ( read-write )  \ BData register
    SAI1 $4 + constant SAI1_ACR1 ( read-write )  \ AConfiguration register 1
    SAI1 $8 + constant SAI1_ACR2 ( read-write )  \ AConfiguration register 2
    SAI1 $C + constant SAI1_AFRCR ( read-write )  \ AFRCR
    SAI1 $10 + constant SAI1_ASLOTR ( read-write )  \ ASlot register
    SAI1 $14 + constant SAI1_AIM ( read-write )  \ AInterrupt mask register2
    SAI1 $18 + constant SAI1_ASR ( read-write )  \ AStatus register
    SAI1 $1C + constant SAI1_ACLRFR ( read-write )  \ AClear flag register
    SAI1 $20 + constant SAI1_ADR ( read-write )  \ AData register
    SAI1 $0 + constant SAI1_GCR ( read-write )  \ Global configuration register
    : SAI1_BCR1. cr ." SAI1_BCR1.  RW   $" SAI1_BCR1 @ hex. SAI1_BCR1 1b. ;
    : SAI1_BCR2. cr ." SAI1_BCR2.  RW   $" SAI1_BCR2 @ hex. SAI1_BCR2 1b. ;
    : SAI1_BFRCR. cr ." SAI1_BFRCR.  RW   $" SAI1_BFRCR @ hex. SAI1_BFRCR 1b. ;
    : SAI1_BSLOTR. cr ." SAI1_BSLOTR.  RW   $" SAI1_BSLOTR @ hex. SAI1_BSLOTR 1b. ;
    : SAI1_BIM. cr ." SAI1_BIM.  RW   $" SAI1_BIM @ hex. SAI1_BIM 1b. ;
    : SAI1_BSR. cr ." SAI1_BSR.  RO   $" SAI1_BSR @ hex. SAI1_BSR 1b. ;
    : SAI1_BCLRFR. cr ." SAI1_BCLRFR " WRITEONLY ; 
    : SAI1_BDR. cr ." SAI1_BDR.  RW   $" SAI1_BDR @ hex. SAI1_BDR 1b. ;
    : SAI1_ACR1. cr ." SAI1_ACR1.  RW   $" SAI1_ACR1 @ hex. SAI1_ACR1 1b. ;
    : SAI1_ACR2. cr ." SAI1_ACR2.  RW   $" SAI1_ACR2 @ hex. SAI1_ACR2 1b. ;
    : SAI1_AFRCR. cr ." SAI1_AFRCR.  RW   $" SAI1_AFRCR @ hex. SAI1_AFRCR 1b. ;
    : SAI1_ASLOTR. cr ." SAI1_ASLOTR.  RW   $" SAI1_ASLOTR @ hex. SAI1_ASLOTR 1b. ;
    : SAI1_AIM. cr ." SAI1_AIM.  RW   $" SAI1_AIM @ hex. SAI1_AIM 1b. ;
    : SAI1_ASR. cr ." SAI1_ASR.  RW   $" SAI1_ASR @ hex. SAI1_ASR 1b. ;
    : SAI1_ACLRFR. cr ." SAI1_ACLRFR.  RW   $" SAI1_ACLRFR @ hex. SAI1_ACLRFR 1b. ;
    : SAI1_ADR. cr ." SAI1_ADR.  RW   $" SAI1_ADR @ hex. SAI1_ADR 1b. ;
    : SAI1_GCR. cr ." SAI1_GCR.  RW   $" SAI1_GCR @ hex. SAI1_GCR 1b. ;
    : SAI1.
      SAI1_BCR1.
      SAI1_BCR2.
      SAI1_BFRCR.
      SAI1_BSLOTR.
      SAI1_BIM.
      SAI1_BSR.
      SAI1_BCLRFR.
      SAI1_BDR.
      SAI1_ACR1.
      SAI1_ACR2.
      SAI1_AFRCR.
      SAI1_ASLOTR.
      SAI1_AIM.
      SAI1_ASR.
      SAI1_ACLRFR.
      SAI1_ADR.
      SAI1_GCR.
    ;
  [then]

  execute-defined? use-SAI2 [if]
    $40015C00 constant SAI2 ( Serial audio interface ) 
    SAI2 $24 + constant SAI2_BCR1 ( read-write )  \ BConfiguration register 1
    SAI2 $28 + constant SAI2_BCR2 ( read-write )  \ BConfiguration register 2
    SAI2 $2C + constant SAI2_BFRCR ( read-write )  \ BFRCR
    SAI2 $30 + constant SAI2_BSLOTR ( read-write )  \ BSlot register
    SAI2 $34 + constant SAI2_BIM ( read-write )  \ BInterrupt mask register2
    SAI2 $38 + constant SAI2_BSR ( read-only )  \ BStatus register
    SAI2 $3C + constant SAI2_BCLRFR ( write-only )  \ BClear flag register
    SAI2 $40 + constant SAI2_BDR ( read-write )  \ BData register
    SAI2 $4 + constant SAI2_ACR1 ( read-write )  \ AConfiguration register 1
    SAI2 $8 + constant SAI2_ACR2 ( read-write )  \ AConfiguration register 2
    SAI2 $C + constant SAI2_AFRCR ( read-write )  \ AFRCR
    SAI2 $10 + constant SAI2_ASLOTR ( read-write )  \ ASlot register
    SAI2 $14 + constant SAI2_AIM ( read-write )  \ AInterrupt mask register2
    SAI2 $18 + constant SAI2_ASR ( read-write )  \ AStatus register
    SAI2 $1C + constant SAI2_ACLRFR ( read-write )  \ AClear flag register
    SAI2 $20 + constant SAI2_ADR ( read-write )  \ AData register
    SAI2 $0 + constant SAI2_GCR ( read-write )  \ Global configuration register
    : SAI2_BCR1. cr ." SAI2_BCR1.  RW   $" SAI2_BCR1 @ hex. SAI2_BCR1 1b. ;
    : SAI2_BCR2. cr ." SAI2_BCR2.  RW   $" SAI2_BCR2 @ hex. SAI2_BCR2 1b. ;
    : SAI2_BFRCR. cr ." SAI2_BFRCR.  RW   $" SAI2_BFRCR @ hex. SAI2_BFRCR 1b. ;
    : SAI2_BSLOTR. cr ." SAI2_BSLOTR.  RW   $" SAI2_BSLOTR @ hex. SAI2_BSLOTR 1b. ;
    : SAI2_BIM. cr ." SAI2_BIM.  RW   $" SAI2_BIM @ hex. SAI2_BIM 1b. ;
    : SAI2_BSR. cr ." SAI2_BSR.  RO   $" SAI2_BSR @ hex. SAI2_BSR 1b. ;
    : SAI2_BCLRFR. cr ." SAI2_BCLRFR " WRITEONLY ; 
    : SAI2_BDR. cr ." SAI2_BDR.  RW   $" SAI2_BDR @ hex. SAI2_BDR 1b. ;
    : SAI2_ACR1. cr ." SAI2_ACR1.  RW   $" SAI2_ACR1 @ hex. SAI2_ACR1 1b. ;
    : SAI2_ACR2. cr ." SAI2_ACR2.  RW   $" SAI2_ACR2 @ hex. SAI2_ACR2 1b. ;
    : SAI2_AFRCR. cr ." SAI2_AFRCR.  RW   $" SAI2_AFRCR @ hex. SAI2_AFRCR 1b. ;
    : SAI2_ASLOTR. cr ." SAI2_ASLOTR.  RW   $" SAI2_ASLOTR @ hex. SAI2_ASLOTR 1b. ;
    : SAI2_AIM. cr ." SAI2_AIM.  RW   $" SAI2_AIM @ hex. SAI2_AIM 1b. ;
    : SAI2_ASR. cr ." SAI2_ASR.  RW   $" SAI2_ASR @ hex. SAI2_ASR 1b. ;
    : SAI2_ACLRFR. cr ." SAI2_ACLRFR.  RW   $" SAI2_ACLRFR @ hex. SAI2_ACLRFR 1b. ;
    : SAI2_ADR. cr ." SAI2_ADR.  RW   $" SAI2_ADR @ hex. SAI2_ADR 1b. ;
    : SAI2_GCR. cr ." SAI2_GCR.  RW   $" SAI2_GCR @ hex. SAI2_GCR 1b. ;
    : SAI2.
      SAI2_BCR1.
      SAI2_BCR2.
      SAI2_BFRCR.
      SAI2_BSLOTR.
      SAI2_BIM.
      SAI2_BSR.
      SAI2_BCLRFR.
      SAI2_BDR.
      SAI2_ACR1.
      SAI2_ACR2.
      SAI2_AFRCR.
      SAI2_ASLOTR.
      SAI2_AIM.
      SAI2_ASR.
      SAI2_ACLRFR.
      SAI2_ADR.
      SAI2_GCR.
    ;
  [then]

  execute-defined? use-DMA2D [if]
    $4002B000 constant DMA2D ( DMA2D controller ) 
    DMA2D $0 + constant DMA2D_CR ( read-write )  \ control register
    DMA2D $4 + constant DMA2D_ISR ( read-only )  \ Interrupt Status Register
    DMA2D $8 + constant DMA2D_IFCR ( read-write )  \ interrupt flag clear register
    DMA2D $C + constant DMA2D_FGMAR ( read-write )  \ foreground memory address register
    DMA2D $10 + constant DMA2D_FGOR ( read-write )  \ foreground offset register
    DMA2D $14 + constant DMA2D_BGMAR ( read-write )  \ background memory address register
    DMA2D $18 + constant DMA2D_BGOR ( read-write )  \ background offset register
    DMA2D $1C + constant DMA2D_FGPFCCR ( read-write )  \ foreground PFC control register
    DMA2D $20 + constant DMA2D_FGCOLR ( read-write )  \ foreground color register
    DMA2D $24 + constant DMA2D_BGPFCCR ( read-write )  \ background PFC control register
    DMA2D $28 + constant DMA2D_BGCOLR ( read-write )  \ background color register
    DMA2D $2C + constant DMA2D_FGCMAR ( read-write )  \ foreground CLUT memory address register
    DMA2D $30 + constant DMA2D_BGCMAR ( read-write )  \ background CLUT memory address register
    DMA2D $34 + constant DMA2D_OPFCCR ( read-write )  \ output PFC control register
    DMA2D $38 + constant DMA2D_OCOLR ( read-write )  \ output color register
    DMA2D $3C + constant DMA2D_OMAR ( read-write )  \ output memory address register
    DMA2D $40 + constant DMA2D_OOR ( read-write )  \ output offset register
    DMA2D $44 + constant DMA2D_NLR ( read-write )  \ number of line register
    DMA2D $48 + constant DMA2D_LWR ( read-write )  \ line watermark register
    DMA2D $4C + constant DMA2D_AMTCR ( read-write )  \ AHB master timer configuration register
    DMA2D $400 + constant DMA2D_FGCLUT ( read-write )  \ FGCLUT
    DMA2D $800 + constant DMA2D_BGCLUT ( read-write )  \ BGCLUT
    : DMA2D_CR. cr ." DMA2D_CR.  RW   $" DMA2D_CR @ hex. DMA2D_CR 1b. ;
    : DMA2D_ISR. cr ." DMA2D_ISR.  RO   $" DMA2D_ISR @ hex. DMA2D_ISR 1b. ;
    : DMA2D_IFCR. cr ." DMA2D_IFCR.  RW   $" DMA2D_IFCR @ hex. DMA2D_IFCR 1b. ;
    : DMA2D_FGMAR. cr ." DMA2D_FGMAR.  RW   $" DMA2D_FGMAR @ hex. DMA2D_FGMAR 1b. ;
    : DMA2D_FGOR. cr ." DMA2D_FGOR.  RW   $" DMA2D_FGOR @ hex. DMA2D_FGOR 1b. ;
    : DMA2D_BGMAR. cr ." DMA2D_BGMAR.  RW   $" DMA2D_BGMAR @ hex. DMA2D_BGMAR 1b. ;
    : DMA2D_BGOR. cr ." DMA2D_BGOR.  RW   $" DMA2D_BGOR @ hex. DMA2D_BGOR 1b. ;
    : DMA2D_FGPFCCR. cr ." DMA2D_FGPFCCR.  RW   $" DMA2D_FGPFCCR @ hex. DMA2D_FGPFCCR 1b. ;
    : DMA2D_FGCOLR. cr ." DMA2D_FGCOLR.  RW   $" DMA2D_FGCOLR @ hex. DMA2D_FGCOLR 1b. ;
    : DMA2D_BGPFCCR. cr ." DMA2D_BGPFCCR.  RW   $" DMA2D_BGPFCCR @ hex. DMA2D_BGPFCCR 1b. ;
    : DMA2D_BGCOLR. cr ." DMA2D_BGCOLR.  RW   $" DMA2D_BGCOLR @ hex. DMA2D_BGCOLR 1b. ;
    : DMA2D_FGCMAR. cr ." DMA2D_FGCMAR.  RW   $" DMA2D_FGCMAR @ hex. DMA2D_FGCMAR 1b. ;
    : DMA2D_BGCMAR. cr ." DMA2D_BGCMAR.  RW   $" DMA2D_BGCMAR @ hex. DMA2D_BGCMAR 1b. ;
    : DMA2D_OPFCCR. cr ." DMA2D_OPFCCR.  RW   $" DMA2D_OPFCCR @ hex. DMA2D_OPFCCR 1b. ;
    : DMA2D_OCOLR. cr ." DMA2D_OCOLR.  RW   $" DMA2D_OCOLR @ hex. DMA2D_OCOLR 1b. ;
    : DMA2D_OMAR. cr ." DMA2D_OMAR.  RW   $" DMA2D_OMAR @ hex. DMA2D_OMAR 1b. ;
    : DMA2D_OOR. cr ." DMA2D_OOR.  RW   $" DMA2D_OOR @ hex. DMA2D_OOR 1b. ;
    : DMA2D_NLR. cr ." DMA2D_NLR.  RW   $" DMA2D_NLR @ hex. DMA2D_NLR 1b. ;
    : DMA2D_LWR. cr ." DMA2D_LWR.  RW   $" DMA2D_LWR @ hex. DMA2D_LWR 1b. ;
    : DMA2D_AMTCR. cr ." DMA2D_AMTCR.  RW   $" DMA2D_AMTCR @ hex. DMA2D_AMTCR 1b. ;
    : DMA2D_FGCLUT. cr ." DMA2D_FGCLUT.  RW   $" DMA2D_FGCLUT @ hex. DMA2D_FGCLUT 1b. ;
    : DMA2D_BGCLUT. cr ." DMA2D_BGCLUT.  RW   $" DMA2D_BGCLUT @ hex. DMA2D_BGCLUT 1b. ;
    : DMA2D.
      DMA2D_CR.
      DMA2D_ISR.
      DMA2D_IFCR.
      DMA2D_FGMAR.
      DMA2D_FGOR.
      DMA2D_BGMAR.
      DMA2D_BGOR.
      DMA2D_FGPFCCR.
      DMA2D_FGCOLR.
      DMA2D_BGPFCCR.
      DMA2D_BGCOLR.
      DMA2D_FGCMAR.
      DMA2D_BGCMAR.
      DMA2D_OPFCCR.
      DMA2D_OCOLR.
      DMA2D_OMAR.
      DMA2D_OOR.
      DMA2D_NLR.
      DMA2D_LWR.
      DMA2D_AMTCR.
      DMA2D_FGCLUT.
      DMA2D_BGCLUT.
    ;
  [then]

  execute-defined? use-QUADSPI [if]
    $A0001000 constant QUADSPI ( QuadSPI interface ) 
    QUADSPI $0 + constant QUADSPI_CR ( read-write )  \ control register
    QUADSPI $4 + constant QUADSPI_DCR ( read-write )  \ device configuration register
    QUADSPI $8 + constant QUADSPI_SR ( read-only )  \ status register
    QUADSPI $C + constant QUADSPI_FCR ( read-write )  \ flag clear register
    QUADSPI $10 + constant QUADSPI_DLR ( read-write )  \ data length register
    QUADSPI $14 + constant QUADSPI_CCR ( read-write )  \ communication configuration register
    QUADSPI $18 + constant QUADSPI_AR ( read-write )  \ address register
    QUADSPI $1C + constant QUADSPI_ABR ( read-write )  \ ABR
    QUADSPI $20 + constant QUADSPI_DR ( read-write )  \ data register
    QUADSPI $24 + constant QUADSPI_PSMKR ( read-write )  \ polling status mask register
    QUADSPI $28 + constant QUADSPI_PSMAR ( read-write )  \ polling status match register
    QUADSPI $2C + constant QUADSPI_PIR ( read-write )  \ polling interval register
    QUADSPI $30 + constant QUADSPI_LPTR ( read-write )  \ low-power timeout register
    : QUADSPI_CR. cr ." QUADSPI_CR.  RW   $" QUADSPI_CR @ hex. QUADSPI_CR 1b. ;
    : QUADSPI_DCR. cr ." QUADSPI_DCR.  RW   $" QUADSPI_DCR @ hex. QUADSPI_DCR 1b. ;
    : QUADSPI_SR. cr ." QUADSPI_SR.  RO   $" QUADSPI_SR @ hex. QUADSPI_SR 1b. ;
    : QUADSPI_FCR. cr ." QUADSPI_FCR.  RW   $" QUADSPI_FCR @ hex. QUADSPI_FCR 1b. ;
    : QUADSPI_DLR. cr ." QUADSPI_DLR.  RW   $" QUADSPI_DLR @ hex. QUADSPI_DLR 1b. ;
    : QUADSPI_CCR. cr ." QUADSPI_CCR.  RW   $" QUADSPI_CCR @ hex. QUADSPI_CCR 1b. ;
    : QUADSPI_AR. cr ." QUADSPI_AR.  RW   $" QUADSPI_AR @ hex. QUADSPI_AR 1b. ;
    : QUADSPI_ABR. cr ." QUADSPI_ABR.  RW   $" QUADSPI_ABR @ hex. QUADSPI_ABR 1b. ;
    : QUADSPI_DR. cr ." QUADSPI_DR.  RW   $" QUADSPI_DR @ hex. QUADSPI_DR 1b. ;
    : QUADSPI_PSMKR. cr ." QUADSPI_PSMKR.  RW   $" QUADSPI_PSMKR @ hex. QUADSPI_PSMKR 1b. ;
    : QUADSPI_PSMAR. cr ." QUADSPI_PSMAR.  RW   $" QUADSPI_PSMAR @ hex. QUADSPI_PSMAR 1b. ;
    : QUADSPI_PIR. cr ." QUADSPI_PIR.  RW   $" QUADSPI_PIR @ hex. QUADSPI_PIR 1b. ;
    : QUADSPI_LPTR. cr ." QUADSPI_LPTR.  RW   $" QUADSPI_LPTR @ hex. QUADSPI_LPTR 1b. ;
    : QUADSPI.
      QUADSPI_CR.
      QUADSPI_DCR.
      QUADSPI_SR.
      QUADSPI_FCR.
      QUADSPI_DLR.
      QUADSPI_CCR.
      QUADSPI_AR.
      QUADSPI_ABR.
      QUADSPI_DR.
      QUADSPI_PSMKR.
      QUADSPI_PSMAR.
      QUADSPI_PIR.
      QUADSPI_LPTR.
    ;
  [then]

  execute-defined? use-CEC [if]
    $40006C00 constant CEC ( HDMI-CEC controller ) 
    CEC $0 + constant CEC_CR ( read-write )  \ control register
    CEC $4 + constant CEC_CFGR ( read-write )  \ configuration register
    CEC $8 + constant CEC_TXDR ( write-only )  \ Tx data register
    CEC $C + constant CEC_RXDR ( read-only )  \ Rx Data Register
    CEC $10 + constant CEC_ISR ( read-write )  \ Interrupt and Status Register
    CEC $14 + constant CEC_IER ( read-write )  \ interrupt enable register
    : CEC_CR. cr ." CEC_CR.  RW   $" CEC_CR @ hex. CEC_CR 1b. ;
    : CEC_CFGR. cr ." CEC_CFGR.  RW   $" CEC_CFGR @ hex. CEC_CFGR 1b. ;
    : CEC_TXDR. cr ." CEC_TXDR " WRITEONLY ; 
    : CEC_RXDR. cr ." CEC_RXDR.  RO   $" CEC_RXDR @ hex. CEC_RXDR 1b. ;
    : CEC_ISR. cr ." CEC_ISR.  RW   $" CEC_ISR @ hex. CEC_ISR 1b. ;
    : CEC_IER. cr ." CEC_IER.  RW   $" CEC_IER @ hex. CEC_IER 1b. ;
    : CEC.
      CEC_CR.
      CEC_CFGR.
      CEC_TXDR.
      CEC_RXDR.
      CEC_ISR.
      CEC_IER.
    ;
  [then]

  execute-defined? use-SPDIF_RX [if]
    $40004000 constant SPDIF_RX ( Receiver Interface ) 
    SPDIF_RX $0 + constant SPDIF_RX_CR ( read-write )  \ Control register
    SPDIF_RX $4 + constant SPDIF_RX_IMR ( read-write )  \ Interrupt mask register
    SPDIF_RX $8 + constant SPDIF_RX_SR ( read-only )  \ Status register
    SPDIF_RX $C + constant SPDIF_RX_IFCR ( write-only )  \ Interrupt Flag Clear register
    SPDIF_RX $10 + constant SPDIF_RX_DR ( read-only )  \ Data input register
    SPDIF_RX $14 + constant SPDIF_RX_CSR ( read-only )  \ Channel Status register
    SPDIF_RX $18 + constant SPDIF_RX_DIR ( read-only )  \ Debug Information register
    : SPDIF_RX_CR. cr ." SPDIF_RX_CR.  RW   $" SPDIF_RX_CR @ hex. SPDIF_RX_CR 1b. ;
    : SPDIF_RX_IMR. cr ." SPDIF_RX_IMR.  RW   $" SPDIF_RX_IMR @ hex. SPDIF_RX_IMR 1b. ;
    : SPDIF_RX_SR. cr ." SPDIF_RX_SR.  RO   $" SPDIF_RX_SR @ hex. SPDIF_RX_SR 1b. ;
    : SPDIF_RX_IFCR. cr ." SPDIF_RX_IFCR " WRITEONLY ; 
    : SPDIF_RX_DR. cr ." SPDIF_RX_DR.  RO   $" SPDIF_RX_DR @ hex. SPDIF_RX_DR 1b. ;
    : SPDIF_RX_CSR. cr ." SPDIF_RX_CSR.  RO   $" SPDIF_RX_CSR @ hex. SPDIF_RX_CSR 1b. ;
    : SPDIF_RX_DIR. cr ." SPDIF_RX_DIR.  RO   $" SPDIF_RX_DIR @ hex. SPDIF_RX_DIR 1b. ;
    : SPDIF_RX.
      SPDIF_RX_CR.
      SPDIF_RX_IMR.
      SPDIF_RX_SR.
      SPDIF_RX_IFCR.
      SPDIF_RX_DR.
      SPDIF_RX_CSR.
      SPDIF_RX_DIR.
    ;
  [then]

  execute-defined? use-SDMMC1 [if]
    $40012C00 constant SDMMC1 ( Secure digital input/output interface ) 
    SDMMC1 $0 + constant SDMMC1_POWER ( read-write )  \ power control register
    SDMMC1 $4 + constant SDMMC1_CLKCR ( read-write )  \ SDI clock control register
    SDMMC1 $8 + constant SDMMC1_ARG ( read-write )  \ argument register
    SDMMC1 $C + constant SDMMC1_CMD ( read-write )  \ command register
    SDMMC1 $10 + constant SDMMC1_RESPCMD ( read-only )  \ command response register
    SDMMC1 $14 + constant SDMMC1_RESP1 ( read-only )  \ response 1..4 register
    SDMMC1 $18 + constant SDMMC1_RESP2 ( read-only )  \ response 1..4 register
    SDMMC1 $1C + constant SDMMC1_RESP3 ( read-only )  \ response 1..4 register
    SDMMC1 $20 + constant SDMMC1_RESP4 ( read-only )  \ response 1..4 register
    SDMMC1 $24 + constant SDMMC1_DTIMER ( read-write )  \ data timer register
    SDMMC1 $28 + constant SDMMC1_DLEN ( read-write )  \ data length register
    SDMMC1 $2C + constant SDMMC1_DCTRL ( read-write )  \ data control register
    SDMMC1 $30 + constant SDMMC1_DCOUNT ( read-only )  \ data counter register
    SDMMC1 $34 + constant SDMMC1_STA ( read-only )  \ status register
    SDMMC1 $38 + constant SDMMC1_ICR ( read-write )  \ interrupt clear register
    SDMMC1 $3C + constant SDMMC1_MASK ( read-write )  \ mask register
    SDMMC1 $48 + constant SDMMC1_FIFOCNT ( read-only )  \ FIFO counter register
    SDMMC1 $80 + constant SDMMC1_FIFO ( read-write )  \ data FIFO register
    : SDMMC1_POWER. cr ." SDMMC1_POWER.  RW   $" SDMMC1_POWER @ hex. SDMMC1_POWER 1b. ;
    : SDMMC1_CLKCR. cr ." SDMMC1_CLKCR.  RW   $" SDMMC1_CLKCR @ hex. SDMMC1_CLKCR 1b. ;
    : SDMMC1_ARG. cr ." SDMMC1_ARG.  RW   $" SDMMC1_ARG @ hex. SDMMC1_ARG 1b. ;
    : SDMMC1_CMD. cr ." SDMMC1_CMD.  RW   $" SDMMC1_CMD @ hex. SDMMC1_CMD 1b. ;
    : SDMMC1_RESPCMD. cr ." SDMMC1_RESPCMD.  RO   $" SDMMC1_RESPCMD @ hex. SDMMC1_RESPCMD 1b. ;
    : SDMMC1_RESP1. cr ." SDMMC1_RESP1.  RO   $" SDMMC1_RESP1 @ hex. SDMMC1_RESP1 1b. ;
    : SDMMC1_RESP2. cr ." SDMMC1_RESP2.  RO   $" SDMMC1_RESP2 @ hex. SDMMC1_RESP2 1b. ;
    : SDMMC1_RESP3. cr ." SDMMC1_RESP3.  RO   $" SDMMC1_RESP3 @ hex. SDMMC1_RESP3 1b. ;
    : SDMMC1_RESP4. cr ." SDMMC1_RESP4.  RO   $" SDMMC1_RESP4 @ hex. SDMMC1_RESP4 1b. ;
    : SDMMC1_DTIMER. cr ." SDMMC1_DTIMER.  RW   $" SDMMC1_DTIMER @ hex. SDMMC1_DTIMER 1b. ;
    : SDMMC1_DLEN. cr ." SDMMC1_DLEN.  RW   $" SDMMC1_DLEN @ hex. SDMMC1_DLEN 1b. ;
    : SDMMC1_DCTRL. cr ." SDMMC1_DCTRL.  RW   $" SDMMC1_DCTRL @ hex. SDMMC1_DCTRL 1b. ;
    : SDMMC1_DCOUNT. cr ." SDMMC1_DCOUNT.  RO   $" SDMMC1_DCOUNT @ hex. SDMMC1_DCOUNT 1b. ;
    : SDMMC1_STA. cr ." SDMMC1_STA.  RO   $" SDMMC1_STA @ hex. SDMMC1_STA 1b. ;
    : SDMMC1_ICR. cr ." SDMMC1_ICR.  RW   $" SDMMC1_ICR @ hex. SDMMC1_ICR 1b. ;
    : SDMMC1_MASK. cr ." SDMMC1_MASK.  RW   $" SDMMC1_MASK @ hex. SDMMC1_MASK 1b. ;
    : SDMMC1_FIFOCNT. cr ." SDMMC1_FIFOCNT.  RO   $" SDMMC1_FIFOCNT @ hex. SDMMC1_FIFOCNT 1b. ;
    : SDMMC1_FIFO. cr ." SDMMC1_FIFO.  RW   $" SDMMC1_FIFO @ hex. SDMMC1_FIFO 1b. ;
    : SDMMC1.
      SDMMC1_POWER.
      SDMMC1_CLKCR.
      SDMMC1_ARG.
      SDMMC1_CMD.
      SDMMC1_RESPCMD.
      SDMMC1_RESP1.
      SDMMC1_RESP2.
      SDMMC1_RESP3.
      SDMMC1_RESP4.
      SDMMC1_DTIMER.
      SDMMC1_DLEN.
      SDMMC1_DCTRL.
      SDMMC1_DCOUNT.
      SDMMC1_STA.
      SDMMC1_ICR.
      SDMMC1_MASK.
      SDMMC1_FIFOCNT.
      SDMMC1_FIFO.
    ;
  [then]

  execute-defined? use-LPTIM1 [if]
    $40002400 constant LPTIM1 ( Low power timer ) 
    LPTIM1 $0 + constant LPTIM1_ISR ( read-only )  \ Interrupt and Status Register
    LPTIM1 $4 + constant LPTIM1_ICR ( write-only )  \ Interrupt Clear Register
    LPTIM1 $8 + constant LPTIM1_IER ( read-write )  \ Interrupt Enable Register
    LPTIM1 $C + constant LPTIM1_CFGR ( read-write )  \ Configuration Register
    LPTIM1 $10 + constant LPTIM1_CR ( read-write )  \ Control Register
    LPTIM1 $14 + constant LPTIM1_CMP ( read-write )  \ Compare Register
    LPTIM1 $18 + constant LPTIM1_ARR ( read-write )  \ Autoreload Register
    LPTIM1 $1C + constant LPTIM1_CNT ( read-only )  \ Counter Register
    : LPTIM1_ISR. cr ." LPTIM1_ISR.  RO   $" LPTIM1_ISR @ hex. LPTIM1_ISR 1b. ;
    : LPTIM1_ICR. cr ." LPTIM1_ICR " WRITEONLY ; 
    : LPTIM1_IER. cr ." LPTIM1_IER.  RW   $" LPTIM1_IER @ hex. LPTIM1_IER 1b. ;
    : LPTIM1_CFGR. cr ." LPTIM1_CFGR.  RW   $" LPTIM1_CFGR @ hex. LPTIM1_CFGR 1b. ;
    : LPTIM1_CR. cr ." LPTIM1_CR.  RW   $" LPTIM1_CR @ hex. LPTIM1_CR 1b. ;
    : LPTIM1_CMP. cr ." LPTIM1_CMP.  RW   $" LPTIM1_CMP @ hex. LPTIM1_CMP 1b. ;
    : LPTIM1_ARR. cr ." LPTIM1_ARR.  RW   $" LPTIM1_ARR @ hex. LPTIM1_ARR 1b. ;
    : LPTIM1_CNT. cr ." LPTIM1_CNT.  RO   $" LPTIM1_CNT @ hex. LPTIM1_CNT 1b. ;
    : LPTIM1.
      LPTIM1_ISR.
      LPTIM1_ICR.
      LPTIM1_IER.
      LPTIM1_CFGR.
      LPTIM1_CR.
      LPTIM1_CMP.
      LPTIM1_ARR.
      LPTIM1_CNT.
    ;
  [then]

  execute-defined? use-I2C1 [if]
    $40005400 constant I2C1 ( Inter-integrated circuit ) 
    I2C1 $0 + constant I2C1_CR1 ( read-write )  \ Control register 1
    I2C1 $4 + constant I2C1_CR2 ( read-write )  \ Control register 2
    I2C1 $8 + constant I2C1_OAR1 ( read-write )  \ Own address register 1
    I2C1 $C + constant I2C1_OAR2 ( read-write )  \ Own address register 2
    I2C1 $10 + constant I2C1_TIMINGR ( read-write )  \ Timing register
    I2C1 $14 + constant I2C1_TIMEOUTR ( read-write )  \ Status register 1
    I2C1 $18 + constant I2C1_ISR (  )  \ Interrupt and Status register
    I2C1 $1C + constant I2C1_ICR ( write-only )  \ Interrupt clear register
    I2C1 $20 + constant I2C1_PECR ( read-only )  \ PEC register
    I2C1 $24 + constant I2C1_RXDR ( read-only )  \ Receive data register
    I2C1 $28 + constant I2C1_TXDR ( read-write )  \ Transmit data register
    : I2C1_CR1. cr ." I2C1_CR1.  RW   $" I2C1_CR1 @ hex. I2C1_CR1 1b. ;
    : I2C1_CR2. cr ." I2C1_CR2.  RW   $" I2C1_CR2 @ hex. I2C1_CR2 1b. ;
    : I2C1_OAR1. cr ." I2C1_OAR1.  RW   $" I2C1_OAR1 @ hex. I2C1_OAR1 1b. ;
    : I2C1_OAR2. cr ." I2C1_OAR2.  RW   $" I2C1_OAR2 @ hex. I2C1_OAR2 1b. ;
    : I2C1_TIMINGR. cr ." I2C1_TIMINGR.  RW   $" I2C1_TIMINGR @ hex. I2C1_TIMINGR 1b. ;
    : I2C1_TIMEOUTR. cr ." I2C1_TIMEOUTR.  RW   $" I2C1_TIMEOUTR @ hex. I2C1_TIMEOUTR 1b. ;
    : I2C1_ISR. cr ." I2C1_ISR.   $" I2C1_ISR @ hex. I2C1_ISR 1b. ;
    : I2C1_ICR. cr ." I2C1_ICR " WRITEONLY ; 
    : I2C1_PECR. cr ." I2C1_PECR.  RO   $" I2C1_PECR @ hex. I2C1_PECR 1b. ;
    : I2C1_RXDR. cr ." I2C1_RXDR.  RO   $" I2C1_RXDR @ hex. I2C1_RXDR 1b. ;
    : I2C1_TXDR. cr ." I2C1_TXDR.  RW   $" I2C1_TXDR @ hex. I2C1_TXDR 1b. ;
    : I2C1.
      I2C1_CR1.
      I2C1_CR2.
      I2C1_OAR1.
      I2C1_OAR2.
      I2C1_TIMINGR.
      I2C1_TIMEOUTR.
      I2C1_ISR.
      I2C1_ICR.
      I2C1_PECR.
      I2C1_RXDR.
      I2C1_TXDR.
    ;
  [then]

  execute-defined? use-I2C2 [if]
    $40005800 constant I2C2 ( Inter-integrated circuit ) 
    I2C2 $0 + constant I2C2_CR1 ( read-write )  \ Control register 1
    I2C2 $4 + constant I2C2_CR2 ( read-write )  \ Control register 2
    I2C2 $8 + constant I2C2_OAR1 ( read-write )  \ Own address register 1
    I2C2 $C + constant I2C2_OAR2 ( read-write )  \ Own address register 2
    I2C2 $10 + constant I2C2_TIMINGR ( read-write )  \ Timing register
    I2C2 $14 + constant I2C2_TIMEOUTR ( read-write )  \ Status register 1
    I2C2 $18 + constant I2C2_ISR (  )  \ Interrupt and Status register
    I2C2 $1C + constant I2C2_ICR ( write-only )  \ Interrupt clear register
    I2C2 $20 + constant I2C2_PECR ( read-only )  \ PEC register
    I2C2 $24 + constant I2C2_RXDR ( read-only )  \ Receive data register
    I2C2 $28 + constant I2C2_TXDR ( read-write )  \ Transmit data register
    : I2C2_CR1. cr ." I2C2_CR1.  RW   $" I2C2_CR1 @ hex. I2C2_CR1 1b. ;
    : I2C2_CR2. cr ." I2C2_CR2.  RW   $" I2C2_CR2 @ hex. I2C2_CR2 1b. ;
    : I2C2_OAR1. cr ." I2C2_OAR1.  RW   $" I2C2_OAR1 @ hex. I2C2_OAR1 1b. ;
    : I2C2_OAR2. cr ." I2C2_OAR2.  RW   $" I2C2_OAR2 @ hex. I2C2_OAR2 1b. ;
    : I2C2_TIMINGR. cr ." I2C2_TIMINGR.  RW   $" I2C2_TIMINGR @ hex. I2C2_TIMINGR 1b. ;
    : I2C2_TIMEOUTR. cr ." I2C2_TIMEOUTR.  RW   $" I2C2_TIMEOUTR @ hex. I2C2_TIMEOUTR 1b. ;
    : I2C2_ISR. cr ." I2C2_ISR.   $" I2C2_ISR @ hex. I2C2_ISR 1b. ;
    : I2C2_ICR. cr ." I2C2_ICR " WRITEONLY ; 
    : I2C2_PECR. cr ." I2C2_PECR.  RO   $" I2C2_PECR @ hex. I2C2_PECR 1b. ;
    : I2C2_RXDR. cr ." I2C2_RXDR.  RO   $" I2C2_RXDR @ hex. I2C2_RXDR 1b. ;
    : I2C2_TXDR. cr ." I2C2_TXDR.  RW   $" I2C2_TXDR @ hex. I2C2_TXDR 1b. ;
    : I2C2.
      I2C2_CR1.
      I2C2_CR2.
      I2C2_OAR1.
      I2C2_OAR2.
      I2C2_TIMINGR.
      I2C2_TIMEOUTR.
      I2C2_ISR.
      I2C2_ICR.
      I2C2_PECR.
      I2C2_RXDR.
      I2C2_TXDR.
    ;
  [then]

  execute-defined? use-I2C3 [if]
    $40005C00 constant I2C3 ( Inter-integrated circuit ) 
    I2C3 $0 + constant I2C3_CR1 ( read-write )  \ Control register 1
    I2C3 $4 + constant I2C3_CR2 ( read-write )  \ Control register 2
    I2C3 $8 + constant I2C3_OAR1 ( read-write )  \ Own address register 1
    I2C3 $C + constant I2C3_OAR2 ( read-write )  \ Own address register 2
    I2C3 $10 + constant I2C3_TIMINGR ( read-write )  \ Timing register
    I2C3 $14 + constant I2C3_TIMEOUTR ( read-write )  \ Status register 1
    I2C3 $18 + constant I2C3_ISR (  )  \ Interrupt and Status register
    I2C3 $1C + constant I2C3_ICR ( write-only )  \ Interrupt clear register
    I2C3 $20 + constant I2C3_PECR ( read-only )  \ PEC register
    I2C3 $24 + constant I2C3_RXDR ( read-only )  \ Receive data register
    I2C3 $28 + constant I2C3_TXDR ( read-write )  \ Transmit data register
    : I2C3_CR1. cr ." I2C3_CR1.  RW   $" I2C3_CR1 @ hex. I2C3_CR1 1b. ;
    : I2C3_CR2. cr ." I2C3_CR2.  RW   $" I2C3_CR2 @ hex. I2C3_CR2 1b. ;
    : I2C3_OAR1. cr ." I2C3_OAR1.  RW   $" I2C3_OAR1 @ hex. I2C3_OAR1 1b. ;
    : I2C3_OAR2. cr ." I2C3_OAR2.  RW   $" I2C3_OAR2 @ hex. I2C3_OAR2 1b. ;
    : I2C3_TIMINGR. cr ." I2C3_TIMINGR.  RW   $" I2C3_TIMINGR @ hex. I2C3_TIMINGR 1b. ;
    : I2C3_TIMEOUTR. cr ." I2C3_TIMEOUTR.  RW   $" I2C3_TIMEOUTR @ hex. I2C3_TIMEOUTR 1b. ;
    : I2C3_ISR. cr ." I2C3_ISR.   $" I2C3_ISR @ hex. I2C3_ISR 1b. ;
    : I2C3_ICR. cr ." I2C3_ICR " WRITEONLY ; 
    : I2C3_PECR. cr ." I2C3_PECR.  RO   $" I2C3_PECR @ hex. I2C3_PECR 1b. ;
    : I2C3_RXDR. cr ." I2C3_RXDR.  RO   $" I2C3_RXDR @ hex. I2C3_RXDR 1b. ;
    : I2C3_TXDR. cr ." I2C3_TXDR.  RW   $" I2C3_TXDR @ hex. I2C3_TXDR 1b. ;
    : I2C3.
      I2C3_CR1.
      I2C3_CR2.
      I2C3_OAR1.
      I2C3_OAR2.
      I2C3_TIMINGR.
      I2C3_TIMEOUTR.
      I2C3_ISR.
      I2C3_ICR.
      I2C3_PECR.
      I2C3_RXDR.
      I2C3_TXDR.
    ;
  [then]

  execute-defined? use-I2C4 [if]
    $40006000 constant I2C4 ( Inter-integrated circuit ) 
    I2C4 $0 + constant I2C4_CR1 ( read-write )  \ Control register 1
    I2C4 $4 + constant I2C4_CR2 ( read-write )  \ Control register 2
    I2C4 $8 + constant I2C4_OAR1 ( read-write )  \ Own address register 1
    I2C4 $C + constant I2C4_OAR2 ( read-write )  \ Own address register 2
    I2C4 $10 + constant I2C4_TIMINGR ( read-write )  \ Timing register
    I2C4 $14 + constant I2C4_TIMEOUTR ( read-write )  \ Status register 1
    I2C4 $18 + constant I2C4_ISR (  )  \ Interrupt and Status register
    I2C4 $1C + constant I2C4_ICR ( write-only )  \ Interrupt clear register
    I2C4 $20 + constant I2C4_PECR ( read-only )  \ PEC register
    I2C4 $24 + constant I2C4_RXDR ( read-only )  \ Receive data register
    I2C4 $28 + constant I2C4_TXDR ( read-write )  \ Transmit data register
    : I2C4_CR1. cr ." I2C4_CR1.  RW   $" I2C4_CR1 @ hex. I2C4_CR1 1b. ;
    : I2C4_CR2. cr ." I2C4_CR2.  RW   $" I2C4_CR2 @ hex. I2C4_CR2 1b. ;
    : I2C4_OAR1. cr ." I2C4_OAR1.  RW   $" I2C4_OAR1 @ hex. I2C4_OAR1 1b. ;
    : I2C4_OAR2. cr ." I2C4_OAR2.  RW   $" I2C4_OAR2 @ hex. I2C4_OAR2 1b. ;
    : I2C4_TIMINGR. cr ." I2C4_TIMINGR.  RW   $" I2C4_TIMINGR @ hex. I2C4_TIMINGR 1b. ;
    : I2C4_TIMEOUTR. cr ." I2C4_TIMEOUTR.  RW   $" I2C4_TIMEOUTR @ hex. I2C4_TIMEOUTR 1b. ;
    : I2C4_ISR. cr ." I2C4_ISR.   $" I2C4_ISR @ hex. I2C4_ISR 1b. ;
    : I2C4_ICR. cr ." I2C4_ICR " WRITEONLY ; 
    : I2C4_PECR. cr ." I2C4_PECR.  RO   $" I2C4_PECR @ hex. I2C4_PECR 1b. ;
    : I2C4_RXDR. cr ." I2C4_RXDR.  RO   $" I2C4_RXDR @ hex. I2C4_RXDR 1b. ;
    : I2C4_TXDR. cr ." I2C4_TXDR.  RW   $" I2C4_TXDR @ hex. I2C4_TXDR 1b. ;
    : I2C4.
      I2C4_CR1.
      I2C4_CR2.
      I2C4_OAR1.
      I2C4_OAR2.
      I2C4_TIMINGR.
      I2C4_TIMEOUTR.
      I2C4_ISR.
      I2C4_ICR.
      I2C4_PECR.
      I2C4_RXDR.
      I2C4_TXDR.
    ;
  [then]

  execute-defined? use-RTC [if]
    $40002800 constant RTC ( Real-time clock ) 
    RTC $0 + constant RTC_TR ( read-write )  \ time register
    RTC $4 + constant RTC_DR ( read-write )  \ date register
    RTC $8 + constant RTC_CR ( read-write )  \ control register
    RTC $C + constant RTC_ISR (  )  \ initialization and status register
    RTC $10 + constant RTC_PRER ( read-write )  \ prescaler register
    RTC $14 + constant RTC_WUTR ( read-write )  \ wakeup timer register
    RTC $1C + constant RTC_ALRMAR ( read-write )  \ alarm A register
    RTC $20 + constant RTC_ALRMBR ( read-write )  \ alarm B register
    RTC $24 + constant RTC_WPR ( write-only )  \ write protection register
    RTC $28 + constant RTC_SSR ( read-only )  \ sub second register
    RTC $2C + constant RTC_SHIFTR ( write-only )  \ shift control register
    RTC $30 + constant RTC_TSTR ( read-only )  \ time stamp time register
    RTC $34 + constant RTC_TSDR ( read-only )  \ time stamp date register
    RTC $38 + constant RTC_TSSSR ( read-only )  \ timestamp sub second register
    RTC $3C + constant RTC_CALR ( read-write )  \ calibration register
    RTC $40 + constant RTC_TAMPCR ( read-write )  \ tamper configuration register
    RTC $44 + constant RTC_ALRMASSR ( read-write )  \ alarm A sub second register
    RTC $48 + constant RTC_ALRMBSSR ( read-write )  \ alarm B sub second register
    RTC $4C + constant RTC_OR ( read-write )  \ option register
    RTC $50 + constant RTC_BKP0R ( read-write )  \ backup register
    RTC $54 + constant RTC_BKP1R ( read-write )  \ backup register
    RTC $58 + constant RTC_BKP2R ( read-write )  \ backup register
    RTC $5C + constant RTC_BKP3R ( read-write )  \ backup register
    RTC $60 + constant RTC_BKP4R ( read-write )  \ backup register
    RTC $64 + constant RTC_BKP5R ( read-write )  \ backup register
    RTC $68 + constant RTC_BKP6R ( read-write )  \ backup register
    RTC $6C + constant RTC_BKP7R ( read-write )  \ backup register
    RTC $70 + constant RTC_BKP8R ( read-write )  \ backup register
    RTC $74 + constant RTC_BKP9R ( read-write )  \ backup register
    RTC $78 + constant RTC_BKP10R ( read-write )  \ backup register
    RTC $7C + constant RTC_BKP11R ( read-write )  \ backup register
    RTC $80 + constant RTC_BKP12R ( read-write )  \ backup register
    RTC $84 + constant RTC_BKP13R ( read-write )  \ backup register
    RTC $88 + constant RTC_BKP14R ( read-write )  \ backup register
    RTC $8C + constant RTC_BKP15R ( read-write )  \ backup register
    RTC $90 + constant RTC_BKP16R ( read-write )  \ backup register
    RTC $94 + constant RTC_BKP17R ( read-write )  \ backup register
    RTC $98 + constant RTC_BKP18R ( read-write )  \ backup register
    RTC $9C + constant RTC_BKP19R ( read-write )  \ backup register
    RTC $A0 + constant RTC_BKP20R ( read-write )  \ backup register
    RTC $A4 + constant RTC_BKP21R ( read-write )  \ backup register
    RTC $A8 + constant RTC_BKP22R ( read-write )  \ backup register
    RTC $AC + constant RTC_BKP23R ( read-write )  \ backup register
    RTC $B0 + constant RTC_BKP24R ( read-write )  \ backup register
    RTC $B4 + constant RTC_BKP25R ( read-write )  \ backup register
    RTC $B8 + constant RTC_BKP26R ( read-write )  \ backup register
    RTC $BC + constant RTC_BKP27R ( read-write )  \ backup register
    RTC $C0 + constant RTC_BKP28R ( read-write )  \ backup register
    RTC $C4 + constant RTC_BKP29R ( read-write )  \ backup register
    RTC $C8 + constant RTC_BKP30R ( read-write )  \ backup register
    RTC $CC + constant RTC_BKP31R ( read-write )  \ backup register
    : RTC_TR. cr ." RTC_TR.  RW   $" RTC_TR @ hex. RTC_TR 1b. ;
    : RTC_DR. cr ." RTC_DR.  RW   $" RTC_DR @ hex. RTC_DR 1b. ;
    : RTC_CR. cr ." RTC_CR.  RW   $" RTC_CR @ hex. RTC_CR 1b. ;
    : RTC_ISR. cr ." RTC_ISR.   $" RTC_ISR @ hex. RTC_ISR 1b. ;
    : RTC_PRER. cr ." RTC_PRER.  RW   $" RTC_PRER @ hex. RTC_PRER 1b. ;
    : RTC_WUTR. cr ." RTC_WUTR.  RW   $" RTC_WUTR @ hex. RTC_WUTR 1b. ;
    : RTC_ALRMAR. cr ." RTC_ALRMAR.  RW   $" RTC_ALRMAR @ hex. RTC_ALRMAR 1b. ;
    : RTC_ALRMBR. cr ." RTC_ALRMBR.  RW   $" RTC_ALRMBR @ hex. RTC_ALRMBR 1b. ;
    : RTC_WPR. cr ." RTC_WPR " WRITEONLY ; 
    : RTC_SSR. cr ." RTC_SSR.  RO   $" RTC_SSR @ hex. RTC_SSR 1b. ;
    : RTC_SHIFTR. cr ." RTC_SHIFTR " WRITEONLY ; 
    : RTC_TSTR. cr ." RTC_TSTR.  RO   $" RTC_TSTR @ hex. RTC_TSTR 1b. ;
    : RTC_TSDR. cr ." RTC_TSDR.  RO   $" RTC_TSDR @ hex. RTC_TSDR 1b. ;
    : RTC_TSSSR. cr ." RTC_TSSSR.  RO   $" RTC_TSSSR @ hex. RTC_TSSSR 1b. ;
    : RTC_CALR. cr ." RTC_CALR.  RW   $" RTC_CALR @ hex. RTC_CALR 1b. ;
    : RTC_TAMPCR. cr ." RTC_TAMPCR.  RW   $" RTC_TAMPCR @ hex. RTC_TAMPCR 1b. ;
    : RTC_ALRMASSR. cr ." RTC_ALRMASSR.  RW   $" RTC_ALRMASSR @ hex. RTC_ALRMASSR 1b. ;
    : RTC_ALRMBSSR. cr ." RTC_ALRMBSSR.  RW   $" RTC_ALRMBSSR @ hex. RTC_ALRMBSSR 1b. ;
    : RTC_OR. cr ." RTC_OR.  RW   $" RTC_OR @ hex. RTC_OR 1b. ;
    : RTC_BKP0R. cr ." RTC_BKP0R.  RW   $" RTC_BKP0R @ hex. RTC_BKP0R 1b. ;
    : RTC_BKP1R. cr ." RTC_BKP1R.  RW   $" RTC_BKP1R @ hex. RTC_BKP1R 1b. ;
    : RTC_BKP2R. cr ." RTC_BKP2R.  RW   $" RTC_BKP2R @ hex. RTC_BKP2R 1b. ;
    : RTC_BKP3R. cr ." RTC_BKP3R.  RW   $" RTC_BKP3R @ hex. RTC_BKP3R 1b. ;
    : RTC_BKP4R. cr ." RTC_BKP4R.  RW   $" RTC_BKP4R @ hex. RTC_BKP4R 1b. ;
    : RTC_BKP5R. cr ." RTC_BKP5R.  RW   $" RTC_BKP5R @ hex. RTC_BKP5R 1b. ;
    : RTC_BKP6R. cr ." RTC_BKP6R.  RW   $" RTC_BKP6R @ hex. RTC_BKP6R 1b. ;
    : RTC_BKP7R. cr ." RTC_BKP7R.  RW   $" RTC_BKP7R @ hex. RTC_BKP7R 1b. ;
    : RTC_BKP8R. cr ." RTC_BKP8R.  RW   $" RTC_BKP8R @ hex. RTC_BKP8R 1b. ;
    : RTC_BKP9R. cr ." RTC_BKP9R.  RW   $" RTC_BKP9R @ hex. RTC_BKP9R 1b. ;
    : RTC_BKP10R. cr ." RTC_BKP10R.  RW   $" RTC_BKP10R @ hex. RTC_BKP10R 1b. ;
    : RTC_BKP11R. cr ." RTC_BKP11R.  RW   $" RTC_BKP11R @ hex. RTC_BKP11R 1b. ;
    : RTC_BKP12R. cr ." RTC_BKP12R.  RW   $" RTC_BKP12R @ hex. RTC_BKP12R 1b. ;
    : RTC_BKP13R. cr ." RTC_BKP13R.  RW   $" RTC_BKP13R @ hex. RTC_BKP13R 1b. ;
    : RTC_BKP14R. cr ." RTC_BKP14R.  RW   $" RTC_BKP14R @ hex. RTC_BKP14R 1b. ;
    : RTC_BKP15R. cr ." RTC_BKP15R.  RW   $" RTC_BKP15R @ hex. RTC_BKP15R 1b. ;
    : RTC_BKP16R. cr ." RTC_BKP16R.  RW   $" RTC_BKP16R @ hex. RTC_BKP16R 1b. ;
    : RTC_BKP17R. cr ." RTC_BKP17R.  RW   $" RTC_BKP17R @ hex. RTC_BKP17R 1b. ;
    : RTC_BKP18R. cr ." RTC_BKP18R.  RW   $" RTC_BKP18R @ hex. RTC_BKP18R 1b. ;
    : RTC_BKP19R. cr ." RTC_BKP19R.  RW   $" RTC_BKP19R @ hex. RTC_BKP19R 1b. ;
    : RTC_BKP20R. cr ." RTC_BKP20R.  RW   $" RTC_BKP20R @ hex. RTC_BKP20R 1b. ;
    : RTC_BKP21R. cr ." RTC_BKP21R.  RW   $" RTC_BKP21R @ hex. RTC_BKP21R 1b. ;
    : RTC_BKP22R. cr ." RTC_BKP22R.  RW   $" RTC_BKP22R @ hex. RTC_BKP22R 1b. ;
    : RTC_BKP23R. cr ." RTC_BKP23R.  RW   $" RTC_BKP23R @ hex. RTC_BKP23R 1b. ;
    : RTC_BKP24R. cr ." RTC_BKP24R.  RW   $" RTC_BKP24R @ hex. RTC_BKP24R 1b. ;
    : RTC_BKP25R. cr ." RTC_BKP25R.  RW   $" RTC_BKP25R @ hex. RTC_BKP25R 1b. ;
    : RTC_BKP26R. cr ." RTC_BKP26R.  RW   $" RTC_BKP26R @ hex. RTC_BKP26R 1b. ;
    : RTC_BKP27R. cr ." RTC_BKP27R.  RW   $" RTC_BKP27R @ hex. RTC_BKP27R 1b. ;
    : RTC_BKP28R. cr ." RTC_BKP28R.  RW   $" RTC_BKP28R @ hex. RTC_BKP28R 1b. ;
    : RTC_BKP29R. cr ." RTC_BKP29R.  RW   $" RTC_BKP29R @ hex. RTC_BKP29R 1b. ;
    : RTC_BKP30R. cr ." RTC_BKP30R.  RW   $" RTC_BKP30R @ hex. RTC_BKP30R 1b. ;
    : RTC_BKP31R. cr ." RTC_BKP31R.  RW   $" RTC_BKP31R @ hex. RTC_BKP31R 1b. ;
    : RTC.
      RTC_TR.
      RTC_DR.
      RTC_CR.
      RTC_ISR.
      RTC_PRER.
      RTC_WUTR.
      RTC_ALRMAR.
      RTC_ALRMBR.
      RTC_WPR.
      RTC_SSR.
      RTC_SHIFTR.
      RTC_TSTR.
      RTC_TSDR.
      RTC_TSSSR.
      RTC_CALR.
      RTC_TAMPCR.
      RTC_ALRMASSR.
      RTC_ALRMBSSR.
      RTC_OR.
      RTC_BKP0R.
      RTC_BKP1R.
      RTC_BKP2R.
      RTC_BKP3R.
      RTC_BKP4R.
      RTC_BKP5R.
      RTC_BKP6R.
      RTC_BKP7R.
      RTC_BKP8R.
      RTC_BKP9R.
      RTC_BKP10R.
      RTC_BKP11R.
      RTC_BKP12R.
      RTC_BKP13R.
      RTC_BKP14R.
      RTC_BKP15R.
      RTC_BKP16R.
      RTC_BKP17R.
      RTC_BKP18R.
      RTC_BKP19R.
      RTC_BKP20R.
      RTC_BKP21R.
      RTC_BKP22R.
      RTC_BKP23R.
      RTC_BKP24R.
      RTC_BKP25R.
      RTC_BKP26R.
      RTC_BKP27R.
      RTC_BKP28R.
      RTC_BKP29R.
      RTC_BKP30R.
      RTC_BKP31R.
    ;
  [then]

  execute-defined? use-USART6 [if]
    $40011400 constant USART6 ( Universal synchronous asynchronous receiver transmitter ) 
    USART6 $0 + constant USART6_CR1 ( read-write )  \ Control register 1
    USART6 $4 + constant USART6_CR2 ( read-write )  \ Control register 2
    USART6 $8 + constant USART6_CR3 ( read-write )  \ Control register 3
    USART6 $C + constant USART6_BRR ( read-write )  \ Baud rate register
    USART6 $10 + constant USART6_GTPR ( read-write )  \ Guard time and prescaler register
    USART6 $14 + constant USART6_RTOR ( read-write )  \ Receiver timeout register
    USART6 $18 + constant USART6_RQR ( write-only )  \ Request register
    USART6 $1C + constant USART6_ISR ( read-only )  \ Interrupt & status register
    USART6 $20 + constant USART6_ICR ( write-only )  \ Interrupt flag clear register
    USART6 $24 + constant USART6_RDR ( read-only )  \ Receive data register
    USART6 $28 + constant USART6_TDR ( read-write )  \ Transmit data register
    : USART6_CR1. cr ." USART6_CR1.  RW   $" USART6_CR1 @ hex. USART6_CR1 1b. ;
    : USART6_CR2. cr ." USART6_CR2.  RW   $" USART6_CR2 @ hex. USART6_CR2 1b. ;
    : USART6_CR3. cr ." USART6_CR3.  RW   $" USART6_CR3 @ hex. USART6_CR3 1b. ;
    : USART6_BRR. cr ." USART6_BRR.  RW   $" USART6_BRR @ hex. USART6_BRR 1b. ;
    : USART6_GTPR. cr ." USART6_GTPR.  RW   $" USART6_GTPR @ hex. USART6_GTPR 1b. ;
    : USART6_RTOR. cr ." USART6_RTOR.  RW   $" USART6_RTOR @ hex. USART6_RTOR 1b. ;
    : USART6_RQR. cr ." USART6_RQR " WRITEONLY ; 
    : USART6_ISR. cr ." USART6_ISR.  RO   $" USART6_ISR @ hex. USART6_ISR 1b. ;
    : USART6_ICR. cr ." USART6_ICR " WRITEONLY ; 
    : USART6_RDR. cr ." USART6_RDR.  RO   $" USART6_RDR @ hex. USART6_RDR 1b. ;
    : USART6_TDR. cr ." USART6_TDR.  RW   $" USART6_TDR @ hex. USART6_TDR 1b. ;
    : USART6.
      USART6_CR1.
      USART6_CR2.
      USART6_CR3.
      USART6_BRR.
      USART6_GTPR.
      USART6_RTOR.
      USART6_RQR.
      USART6_ISR.
      USART6_ICR.
      USART6_RDR.
      USART6_TDR.
    ;
  [then]

  execute-defined? use-USART1 [if]
    $40011000 constant USART1 ( Universal synchronous asynchronous receiver transmitter ) 
    USART1 $0 + constant USART1_CR1 ( read-write )  \ Control register 1
    USART1 $4 + constant USART1_CR2 ( read-write )  \ Control register 2
    USART1 $8 + constant USART1_CR3 ( read-write )  \ Control register 3
    USART1 $C + constant USART1_BRR ( read-write )  \ Baud rate register
    USART1 $10 + constant USART1_GTPR ( read-write )  \ Guard time and prescaler register
    USART1 $14 + constant USART1_RTOR ( read-write )  \ Receiver timeout register
    USART1 $18 + constant USART1_RQR ( write-only )  \ Request register
    USART1 $1C + constant USART1_ISR ( read-only )  \ Interrupt & status register
    USART1 $20 + constant USART1_ICR ( write-only )  \ Interrupt flag clear register
    USART1 $24 + constant USART1_RDR ( read-only )  \ Receive data register
    USART1 $28 + constant USART1_TDR ( read-write )  \ Transmit data register
    : USART1_CR1. cr ." USART1_CR1.  RW   $" USART1_CR1 @ hex. USART1_CR1 1b. ;
    : USART1_CR2. cr ." USART1_CR2.  RW   $" USART1_CR2 @ hex. USART1_CR2 1b. ;
    : USART1_CR3. cr ." USART1_CR3.  RW   $" USART1_CR3 @ hex. USART1_CR3 1b. ;
    : USART1_BRR. cr ." USART1_BRR.  RW   $" USART1_BRR @ hex. USART1_BRR 1b. ;
    : USART1_GTPR. cr ." USART1_GTPR.  RW   $" USART1_GTPR @ hex. USART1_GTPR 1b. ;
    : USART1_RTOR. cr ." USART1_RTOR.  RW   $" USART1_RTOR @ hex. USART1_RTOR 1b. ;
    : USART1_RQR. cr ." USART1_RQR " WRITEONLY ; 
    : USART1_ISR. cr ." USART1_ISR.  RO   $" USART1_ISR @ hex. USART1_ISR 1b. ;
    : USART1_ICR. cr ." USART1_ICR " WRITEONLY ; 
    : USART1_RDR. cr ." USART1_RDR.  RO   $" USART1_RDR @ hex. USART1_RDR 1b. ;
    : USART1_TDR. cr ." USART1_TDR.  RW   $" USART1_TDR @ hex. USART1_TDR 1b. ;
    : USART1.
      USART1_CR1.
      USART1_CR2.
      USART1_CR3.
      USART1_BRR.
      USART1_GTPR.
      USART1_RTOR.
      USART1_RQR.
      USART1_ISR.
      USART1_ICR.
      USART1_RDR.
      USART1_TDR.
    ;
  [then]

  execute-defined? use-USART3 [if]
    $40004800 constant USART3 ( Universal synchronous asynchronous receiver transmitter ) 
    USART3 $0 + constant USART3_CR1 ( read-write )  \ Control register 1
    USART3 $4 + constant USART3_CR2 ( read-write )  \ Control register 2
    USART3 $8 + constant USART3_CR3 ( read-write )  \ Control register 3
    USART3 $C + constant USART3_BRR ( read-write )  \ Baud rate register
    USART3 $10 + constant USART3_GTPR ( read-write )  \ Guard time and prescaler register
    USART3 $14 + constant USART3_RTOR ( read-write )  \ Receiver timeout register
    USART3 $18 + constant USART3_RQR ( write-only )  \ Request register
    USART3 $1C + constant USART3_ISR ( read-only )  \ Interrupt & status register
    USART3 $20 + constant USART3_ICR ( write-only )  \ Interrupt flag clear register
    USART3 $24 + constant USART3_RDR ( read-only )  \ Receive data register
    USART3 $28 + constant USART3_TDR ( read-write )  \ Transmit data register
    : USART3_CR1. cr ." USART3_CR1.  RW   $" USART3_CR1 @ hex. USART3_CR1 1b. ;
    : USART3_CR2. cr ." USART3_CR2.  RW   $" USART3_CR2 @ hex. USART3_CR2 1b. ;
    : USART3_CR3. cr ." USART3_CR3.  RW   $" USART3_CR3 @ hex. USART3_CR3 1b. ;
    : USART3_BRR. cr ." USART3_BRR.  RW   $" USART3_BRR @ hex. USART3_BRR 1b. ;
    : USART3_GTPR. cr ." USART3_GTPR.  RW   $" USART3_GTPR @ hex. USART3_GTPR 1b. ;
    : USART3_RTOR. cr ." USART3_RTOR.  RW   $" USART3_RTOR @ hex. USART3_RTOR 1b. ;
    : USART3_RQR. cr ." USART3_RQR " WRITEONLY ; 
    : USART3_ISR. cr ." USART3_ISR.  RO   $" USART3_ISR @ hex. USART3_ISR 1b. ;
    : USART3_ICR. cr ." USART3_ICR " WRITEONLY ; 
    : USART3_RDR. cr ." USART3_RDR.  RO   $" USART3_RDR @ hex. USART3_RDR 1b. ;
    : USART3_TDR. cr ." USART3_TDR.  RW   $" USART3_TDR @ hex. USART3_TDR 1b. ;
    : USART3.
      USART3_CR1.
      USART3_CR2.
      USART3_CR3.
      USART3_BRR.
      USART3_GTPR.
      USART3_RTOR.
      USART3_RQR.
      USART3_ISR.
      USART3_ICR.
      USART3_RDR.
      USART3_TDR.
    ;
  [then]

  execute-defined? use-USART2 [if]
    $40004400 constant USART2 ( Universal synchronous asynchronous receiver transmitter ) 
    USART2 $0 + constant USART2_CR1 ( read-write )  \ Control register 1
    USART2 $4 + constant USART2_CR2 ( read-write )  \ Control register 2
    USART2 $8 + constant USART2_CR3 ( read-write )  \ Control register 3
    USART2 $C + constant USART2_BRR ( read-write )  \ Baud rate register
    USART2 $10 + constant USART2_GTPR ( read-write )  \ Guard time and prescaler register
    USART2 $14 + constant USART2_RTOR ( read-write )  \ Receiver timeout register
    USART2 $18 + constant USART2_RQR ( write-only )  \ Request register
    USART2 $1C + constant USART2_ISR ( read-only )  \ Interrupt & status register
    USART2 $20 + constant USART2_ICR ( write-only )  \ Interrupt flag clear register
    USART2 $24 + constant USART2_RDR ( read-only )  \ Receive data register
    USART2 $28 + constant USART2_TDR ( read-write )  \ Transmit data register
    : USART2_CR1. cr ." USART2_CR1.  RW   $" USART2_CR1 @ hex. USART2_CR1 1b. ;
    : USART2_CR2. cr ." USART2_CR2.  RW   $" USART2_CR2 @ hex. USART2_CR2 1b. ;
    : USART2_CR3. cr ." USART2_CR3.  RW   $" USART2_CR3 @ hex. USART2_CR3 1b. ;
    : USART2_BRR. cr ." USART2_BRR.  RW   $" USART2_BRR @ hex. USART2_BRR 1b. ;
    : USART2_GTPR. cr ." USART2_GTPR.  RW   $" USART2_GTPR @ hex. USART2_GTPR 1b. ;
    : USART2_RTOR. cr ." USART2_RTOR.  RW   $" USART2_RTOR @ hex. USART2_RTOR 1b. ;
    : USART2_RQR. cr ." USART2_RQR " WRITEONLY ; 
    : USART2_ISR. cr ." USART2_ISR.  RO   $" USART2_ISR @ hex. USART2_ISR 1b. ;
    : USART2_ICR. cr ." USART2_ICR " WRITEONLY ; 
    : USART2_RDR. cr ." USART2_RDR.  RO   $" USART2_RDR @ hex. USART2_RDR 1b. ;
    : USART2_TDR. cr ." USART2_TDR.  RW   $" USART2_TDR @ hex. USART2_TDR 1b. ;
    : USART2.
      USART2_CR1.
      USART2_CR2.
      USART2_CR3.
      USART2_BRR.
      USART2_GTPR.
      USART2_RTOR.
      USART2_RQR.
      USART2_ISR.
      USART2_ICR.
      USART2_RDR.
      USART2_TDR.
    ;
  [then]

  execute-defined? use-UART5 [if]
    $40005000 constant UART5 ( Universal synchronous asynchronous receiver transmitter ) 
    UART5 $0 + constant UART5_CR1 ( read-write )  \ Control register 1
    UART5 $4 + constant UART5_CR2 ( read-write )  \ Control register 2
    UART5 $8 + constant UART5_CR3 ( read-write )  \ Control register 3
    UART5 $C + constant UART5_BRR ( read-write )  \ Baud rate register
    UART5 $10 + constant UART5_GTPR ( read-write )  \ Guard time and prescaler register
    UART5 $14 + constant UART5_RTOR ( read-write )  \ Receiver timeout register
    UART5 $18 + constant UART5_RQR ( write-only )  \ Request register
    UART5 $1C + constant UART5_ISR ( read-only )  \ Interrupt & status register
    UART5 $20 + constant UART5_ICR ( write-only )  \ Interrupt flag clear register
    UART5 $24 + constant UART5_RDR ( read-only )  \ Receive data register
    UART5 $28 + constant UART5_TDR ( read-write )  \ Transmit data register
    : UART5_CR1. cr ." UART5_CR1.  RW   $" UART5_CR1 @ hex. UART5_CR1 1b. ;
    : UART5_CR2. cr ." UART5_CR2.  RW   $" UART5_CR2 @ hex. UART5_CR2 1b. ;
    : UART5_CR3. cr ." UART5_CR3.  RW   $" UART5_CR3 @ hex. UART5_CR3 1b. ;
    : UART5_BRR. cr ." UART5_BRR.  RW   $" UART5_BRR @ hex. UART5_BRR 1b. ;
    : UART5_GTPR. cr ." UART5_GTPR.  RW   $" UART5_GTPR @ hex. UART5_GTPR 1b. ;
    : UART5_RTOR. cr ." UART5_RTOR.  RW   $" UART5_RTOR @ hex. UART5_RTOR 1b. ;
    : UART5_RQR. cr ." UART5_RQR " WRITEONLY ; 
    : UART5_ISR. cr ." UART5_ISR.  RO   $" UART5_ISR @ hex. UART5_ISR 1b. ;
    : UART5_ICR. cr ." UART5_ICR " WRITEONLY ; 
    : UART5_RDR. cr ." UART5_RDR.  RO   $" UART5_RDR @ hex. UART5_RDR 1b. ;
    : UART5_TDR. cr ." UART5_TDR.  RW   $" UART5_TDR @ hex. UART5_TDR 1b. ;
    : UART5.
      UART5_CR1.
      UART5_CR2.
      UART5_CR3.
      UART5_BRR.
      UART5_GTPR.
      UART5_RTOR.
      UART5_RQR.
      UART5_ISR.
      UART5_ICR.
      UART5_RDR.
      UART5_TDR.
    ;
  [then]

  execute-defined? use-UART4 [if]
    $40004C00 constant UART4 ( Universal synchronous asynchronous receiver transmitter ) 
    UART4 $0 + constant UART4_CR1 ( read-write )  \ Control register 1
    UART4 $4 + constant UART4_CR2 ( read-write )  \ Control register 2
    UART4 $8 + constant UART4_CR3 ( read-write )  \ Control register 3
    UART4 $C + constant UART4_BRR ( read-write )  \ Baud rate register
    UART4 $10 + constant UART4_GTPR ( read-write )  \ Guard time and prescaler register
    UART4 $14 + constant UART4_RTOR ( read-write )  \ Receiver timeout register
    UART4 $18 + constant UART4_RQR ( write-only )  \ Request register
    UART4 $1C + constant UART4_ISR ( read-only )  \ Interrupt & status register
    UART4 $20 + constant UART4_ICR ( write-only )  \ Interrupt flag clear register
    UART4 $24 + constant UART4_RDR ( read-only )  \ Receive data register
    UART4 $28 + constant UART4_TDR ( read-write )  \ Transmit data register
    : UART4_CR1. cr ." UART4_CR1.  RW   $" UART4_CR1 @ hex. UART4_CR1 1b. ;
    : UART4_CR2. cr ." UART4_CR2.  RW   $" UART4_CR2 @ hex. UART4_CR2 1b. ;
    : UART4_CR3. cr ." UART4_CR3.  RW   $" UART4_CR3 @ hex. UART4_CR3 1b. ;
    : UART4_BRR. cr ." UART4_BRR.  RW   $" UART4_BRR @ hex. UART4_BRR 1b. ;
    : UART4_GTPR. cr ." UART4_GTPR.  RW   $" UART4_GTPR @ hex. UART4_GTPR 1b. ;
    : UART4_RTOR. cr ." UART4_RTOR.  RW   $" UART4_RTOR @ hex. UART4_RTOR 1b. ;
    : UART4_RQR. cr ." UART4_RQR " WRITEONLY ; 
    : UART4_ISR. cr ." UART4_ISR.  RO   $" UART4_ISR @ hex. UART4_ISR 1b. ;
    : UART4_ICR. cr ." UART4_ICR " WRITEONLY ; 
    : UART4_RDR. cr ." UART4_RDR.  RO   $" UART4_RDR @ hex. UART4_RDR 1b. ;
    : UART4_TDR. cr ." UART4_TDR.  RW   $" UART4_TDR @ hex. UART4_TDR 1b. ;
    : UART4.
      UART4_CR1.
      UART4_CR2.
      UART4_CR3.
      UART4_BRR.
      UART4_GTPR.
      UART4_RTOR.
      UART4_RQR.
      UART4_ISR.
      UART4_ICR.
      UART4_RDR.
      UART4_TDR.
    ;
  [then]

  execute-defined? use-UART8 [if]
    $40007C00 constant UART8 ( Universal synchronous asynchronous receiver transmitter ) 
    UART8 $0 + constant UART8_CR1 ( read-write )  \ Control register 1
    UART8 $4 + constant UART8_CR2 ( read-write )  \ Control register 2
    UART8 $8 + constant UART8_CR3 ( read-write )  \ Control register 3
    UART8 $C + constant UART8_BRR ( read-write )  \ Baud rate register
    UART8 $10 + constant UART8_GTPR ( read-write )  \ Guard time and prescaler register
    UART8 $14 + constant UART8_RTOR ( read-write )  \ Receiver timeout register
    UART8 $18 + constant UART8_RQR ( write-only )  \ Request register
    UART8 $1C + constant UART8_ISR ( read-only )  \ Interrupt & status register
    UART8 $20 + constant UART8_ICR ( write-only )  \ Interrupt flag clear register
    UART8 $24 + constant UART8_RDR ( read-only )  \ Receive data register
    UART8 $28 + constant UART8_TDR ( read-write )  \ Transmit data register
    : UART8_CR1. cr ." UART8_CR1.  RW   $" UART8_CR1 @ hex. UART8_CR1 1b. ;
    : UART8_CR2. cr ." UART8_CR2.  RW   $" UART8_CR2 @ hex. UART8_CR2 1b. ;
    : UART8_CR3. cr ." UART8_CR3.  RW   $" UART8_CR3 @ hex. UART8_CR3 1b. ;
    : UART8_BRR. cr ." UART8_BRR.  RW   $" UART8_BRR @ hex. UART8_BRR 1b. ;
    : UART8_GTPR. cr ." UART8_GTPR.  RW   $" UART8_GTPR @ hex. UART8_GTPR 1b. ;
    : UART8_RTOR. cr ." UART8_RTOR.  RW   $" UART8_RTOR @ hex. UART8_RTOR 1b. ;
    : UART8_RQR. cr ." UART8_RQR " WRITEONLY ; 
    : UART8_ISR. cr ." UART8_ISR.  RO   $" UART8_ISR @ hex. UART8_ISR 1b. ;
    : UART8_ICR. cr ." UART8_ICR " WRITEONLY ; 
    : UART8_RDR. cr ." UART8_RDR.  RO   $" UART8_RDR @ hex. UART8_RDR 1b. ;
    : UART8_TDR. cr ." UART8_TDR.  RW   $" UART8_TDR @ hex. UART8_TDR 1b. ;
    : UART8.
      UART8_CR1.
      UART8_CR2.
      UART8_CR3.
      UART8_BRR.
      UART8_GTPR.
      UART8_RTOR.
      UART8_RQR.
      UART8_ISR.
      UART8_ICR.
      UART8_RDR.
      UART8_TDR.
    ;
  [then]

  execute-defined? use-UART7 [if]
    $40007800 constant UART7 ( Universal synchronous asynchronous receiver transmitter ) 
    UART7 $0 + constant UART7_CR1 ( read-write )  \ Control register 1
    UART7 $4 + constant UART7_CR2 ( read-write )  \ Control register 2
    UART7 $8 + constant UART7_CR3 ( read-write )  \ Control register 3
    UART7 $C + constant UART7_BRR ( read-write )  \ Baud rate register
    UART7 $10 + constant UART7_GTPR ( read-write )  \ Guard time and prescaler register
    UART7 $14 + constant UART7_RTOR ( read-write )  \ Receiver timeout register
    UART7 $18 + constant UART7_RQR ( write-only )  \ Request register
    UART7 $1C + constant UART7_ISR ( read-only )  \ Interrupt & status register
    UART7 $20 + constant UART7_ICR ( write-only )  \ Interrupt flag clear register
    UART7 $24 + constant UART7_RDR ( read-only )  \ Receive data register
    UART7 $28 + constant UART7_TDR ( read-write )  \ Transmit data register
    : UART7_CR1. cr ." UART7_CR1.  RW   $" UART7_CR1 @ hex. UART7_CR1 1b. ;
    : UART7_CR2. cr ." UART7_CR2.  RW   $" UART7_CR2 @ hex. UART7_CR2 1b. ;
    : UART7_CR3. cr ." UART7_CR3.  RW   $" UART7_CR3 @ hex. UART7_CR3 1b. ;
    : UART7_BRR. cr ." UART7_BRR.  RW   $" UART7_BRR @ hex. UART7_BRR 1b. ;
    : UART7_GTPR. cr ." UART7_GTPR.  RW   $" UART7_GTPR @ hex. UART7_GTPR 1b. ;
    : UART7_RTOR. cr ." UART7_RTOR.  RW   $" UART7_RTOR @ hex. UART7_RTOR 1b. ;
    : UART7_RQR. cr ." UART7_RQR " WRITEONLY ; 
    : UART7_ISR. cr ." UART7_ISR.  RO   $" UART7_ISR @ hex. UART7_ISR 1b. ;
    : UART7_ICR. cr ." UART7_ICR " WRITEONLY ; 
    : UART7_RDR. cr ." UART7_RDR.  RO   $" UART7_RDR @ hex. UART7_RDR 1b. ;
    : UART7_TDR. cr ." UART7_TDR.  RW   $" UART7_TDR @ hex. UART7_TDR 1b. ;
    : UART7.
      UART7_CR1.
      UART7_CR2.
      UART7_CR3.
      UART7_BRR.
      UART7_GTPR.
      UART7_RTOR.
      UART7_RQR.
      UART7_ISR.
      UART7_ICR.
      UART7_RDR.
      UART7_TDR.
    ;
  [then]

  execute-defined? use-OTG_FS_GLOBAL [if]
    $50000000 constant OTG_FS_GLOBAL ( USB on the go full speed ) 
    OTG_FS_GLOBAL $0 + constant OTG_FS_GLOBAL_OTG_FS_GOTGCTL (  )  \ OTG_FS control and status register OTG_FS_GOTGCTL
    OTG_FS_GLOBAL $4 + constant OTG_FS_GLOBAL_OTG_FS_GOTGINT ( read-write )  \ OTG_FS interrupt register OTG_FS_GOTGINT
    OTG_FS_GLOBAL $8 + constant OTG_FS_GLOBAL_OTG_FS_GAHBCFG ( read-write )  \ OTG_FS AHB configuration register OTG_FS_GAHBCFG
    OTG_FS_GLOBAL $C + constant OTG_FS_GLOBAL_OTG_FS_GUSBCFG (  )  \ OTG_FS USB configuration register OTG_FS_GUSBCFG
    OTG_FS_GLOBAL $10 + constant OTG_FS_GLOBAL_OTG_FS_GRSTCTL (  )  \ OTG_FS reset register OTG_FS_GRSTCTL
    OTG_FS_GLOBAL $14 + constant OTG_FS_GLOBAL_OTG_FS_GINTSTS (  )  \ OTG_FS core interrupt register OTG_FS_GINTSTS
    OTG_FS_GLOBAL $18 + constant OTG_FS_GLOBAL_OTG_FS_GINTMSK (  )  \ OTG_FS interrupt mask register OTG_FS_GINTMSK
    OTG_FS_GLOBAL $1C + constant OTG_FS_GLOBAL_OTG_FS_GRXSTSR_Device ( read-only )  \ OTG_FS Receive status debug readDevice mode
    OTG_FS_GLOBAL $1C + constant OTG_FS_GLOBAL_OTG_FS_GRXSTSR_Host ( read-only )  \ OTG_FS Receive status debug readHost mode
    OTG_FS_GLOBAL $24 + constant OTG_FS_GLOBAL_OTG_FS_GRXFSIZ ( read-write )  \ OTG_FS Receive FIFO size register OTG_FS_GRXFSIZ
    OTG_FS_GLOBAL $28 + constant OTG_FS_GLOBAL_OTG_FS_DIEPTXF0_Device ( read-write )  \ OTG_FS Endpoint 0 Transmit FIFO size
    OTG_FS_GLOBAL $28 + constant OTG_FS_GLOBAL_OTG_FS_HNPTXFSIZ_Host ( read-write )  \ OTG_FS Host non-periodic transmit FIFO size register
    OTG_FS_GLOBAL $2C + constant OTG_FS_GLOBAL_OTG_FS_HNPTXSTS ( read-only )  \ OTG_FS non-periodic transmit FIFO/queue status register OTG_FS_GNPTXSTS
    OTG_FS_GLOBAL $38 + constant OTG_FS_GLOBAL_OTG_FS_GCCFG ( read-write )  \ OTG_FS general core configuration register OTG_FS_GCCFG
    OTG_FS_GLOBAL $3C + constant OTG_FS_GLOBAL_OTG_FS_CID ( read-write )  \ core ID register
    OTG_FS_GLOBAL $100 + constant OTG_FS_GLOBAL_OTG_FS_HPTXFSIZ ( read-write )  \ OTG_FS Host periodic transmit FIFO size register OTG_FS_HPTXFSIZ
    OTG_FS_GLOBAL $104 + constant OTG_FS_GLOBAL_OTG_FS_DIEPTXF1 ( read-write )  \ OTG_FS device IN endpoint transmit FIFO size register OTG_FS_DIEPTXF1
    OTG_FS_GLOBAL $108 + constant OTG_FS_GLOBAL_OTG_FS_DIEPTXF2 ( read-write )  \ OTG_FS device IN endpoint transmit FIFO size register OTG_FS_DIEPTXF2
    OTG_FS_GLOBAL $10C + constant OTG_FS_GLOBAL_OTG_FS_DIEPTXF3 ( read-write )  \ OTG_FS device IN endpoint transmit FIFO size register OTG_FS_DIEPTXF3
    OTG_FS_GLOBAL $20 + constant OTG_FS_GLOBAL_OTG_FS_GRXSTSP_Device ( read-only )  \ OTG status read and pop register Device mode
    OTG_FS_GLOBAL $20 + constant OTG_FS_GLOBAL_OTG_FS_GRXSTSP_Host ( read-only )  \ OTG status read and pop register Host mode
    OTG_FS_GLOBAL $30 + constant OTG_FS_GLOBAL_OTG_FS_GI2CCTL ( read-write )  \ OTG I2C access register
    OTG_FS_GLOBAL $58 + constant OTG_FS_GLOBAL_OTG_FS_GPWRDN ( read-write )  \ OTG power down register
    OTG_FS_GLOBAL $60 + constant OTG_FS_GLOBAL_OTG_FS_GADPCTL (  )  \ OTG ADP timer, control and status register
    OTG_FS_GLOBAL $110 + constant OTG_FS_GLOBAL_OTG_FS_DIEPTXF4 ( read-write )  \ OTG_FS device IN endpoint transmit FIFO size register OTG_FS_DIEPTXF4
    OTG_FS_GLOBAL $114 + constant OTG_FS_GLOBAL_OTG_FS_DIEPTXF5 ( read-write )  \ OTG_FS device IN endpoint transmit FIFO size register OTG_FS_DIEPTXF5
    OTG_FS_GLOBAL $54 + constant OTG_FS_GLOBAL_OTG_FS_GLPMCFG (  )  \ OTG core LPM configuration register
    : OTG_FS_GLOBAL_OTG_FS_GOTGCTL. cr ." OTG_FS_GLOBAL_OTG_FS_GOTGCTL.   $" OTG_FS_GLOBAL_OTG_FS_GOTGCTL @ hex. OTG_FS_GLOBAL_OTG_FS_GOTGCTL 1b. ;
    : OTG_FS_GLOBAL_OTG_FS_GOTGINT. cr ." OTG_FS_GLOBAL_OTG_FS_GOTGINT.  RW   $" OTG_FS_GLOBAL_OTG_FS_GOTGINT @ hex. OTG_FS_GLOBAL_OTG_FS_GOTGINT 1b. ;
    : OTG_FS_GLOBAL_OTG_FS_GAHBCFG. cr ." OTG_FS_GLOBAL_OTG_FS_GAHBCFG.  RW   $" OTG_FS_GLOBAL_OTG_FS_GAHBCFG @ hex. OTG_FS_GLOBAL_OTG_FS_GAHBCFG 1b. ;
    : OTG_FS_GLOBAL_OTG_FS_GUSBCFG. cr ." OTG_FS_GLOBAL_OTG_FS_GUSBCFG.   $" OTG_FS_GLOBAL_OTG_FS_GUSBCFG @ hex. OTG_FS_GLOBAL_OTG_FS_GUSBCFG 1b. ;
    : OTG_FS_GLOBAL_OTG_FS_GRSTCTL. cr ." OTG_FS_GLOBAL_OTG_FS_GRSTCTL.   $" OTG_FS_GLOBAL_OTG_FS_GRSTCTL @ hex. OTG_FS_GLOBAL_OTG_FS_GRSTCTL 1b. ;
    : OTG_FS_GLOBAL_OTG_FS_GINTSTS. cr ." OTG_FS_GLOBAL_OTG_FS_GINTSTS.   $" OTG_FS_GLOBAL_OTG_FS_GINTSTS @ hex. OTG_FS_GLOBAL_OTG_FS_GINTSTS 1b. ;
    : OTG_FS_GLOBAL_OTG_FS_GINTMSK. cr ." OTG_FS_GLOBAL_OTG_FS_GINTMSK.   $" OTG_FS_GLOBAL_OTG_FS_GINTMSK @ hex. OTG_FS_GLOBAL_OTG_FS_GINTMSK 1b. ;
    : OTG_FS_GLOBAL_OTG_FS_GRXSTSR_Device. cr ." OTG_FS_GLOBAL_OTG_FS_GRXSTSR_Device.  RO   $" OTG_FS_GLOBAL_OTG_FS_GRXSTSR_Device @ hex. OTG_FS_GLOBAL_OTG_FS_GRXSTSR_Device 1b. ;
    : OTG_FS_GLOBAL_OTG_FS_GRXSTSR_Host. cr ." OTG_FS_GLOBAL_OTG_FS_GRXSTSR_Host.  RO   $" OTG_FS_GLOBAL_OTG_FS_GRXSTSR_Host @ hex. OTG_FS_GLOBAL_OTG_FS_GRXSTSR_Host 1b. ;
    : OTG_FS_GLOBAL_OTG_FS_GRXFSIZ. cr ." OTG_FS_GLOBAL_OTG_FS_GRXFSIZ.  RW   $" OTG_FS_GLOBAL_OTG_FS_GRXFSIZ @ hex. OTG_FS_GLOBAL_OTG_FS_GRXFSIZ 1b. ;
    : OTG_FS_GLOBAL_OTG_FS_DIEPTXF0_Device. cr ." OTG_FS_GLOBAL_OTG_FS_DIEPTXF0_Device.  RW   $" OTG_FS_GLOBAL_OTG_FS_DIEPTXF0_Device @ hex. OTG_FS_GLOBAL_OTG_FS_DIEPTXF0_Device 1b. ;
    : OTG_FS_GLOBAL_OTG_FS_HNPTXFSIZ_Host. cr ." OTG_FS_GLOBAL_OTG_FS_HNPTXFSIZ_Host.  RW   $" OTG_FS_GLOBAL_OTG_FS_HNPTXFSIZ_Host @ hex. OTG_FS_GLOBAL_OTG_FS_HNPTXFSIZ_Host 1b. ;
    : OTG_FS_GLOBAL_OTG_FS_HNPTXSTS. cr ." OTG_FS_GLOBAL_OTG_FS_HNPTXSTS.  RO   $" OTG_FS_GLOBAL_OTG_FS_HNPTXSTS @ hex. OTG_FS_GLOBAL_OTG_FS_HNPTXSTS 1b. ;
    : OTG_FS_GLOBAL_OTG_FS_GCCFG. cr ." OTG_FS_GLOBAL_OTG_FS_GCCFG.  RW   $" OTG_FS_GLOBAL_OTG_FS_GCCFG @ hex. OTG_FS_GLOBAL_OTG_FS_GCCFG 1b. ;
    : OTG_FS_GLOBAL_OTG_FS_CID. cr ." OTG_FS_GLOBAL_OTG_FS_CID.  RW   $" OTG_FS_GLOBAL_OTG_FS_CID @ hex. OTG_FS_GLOBAL_OTG_FS_CID 1b. ;
    : OTG_FS_GLOBAL_OTG_FS_HPTXFSIZ. cr ." OTG_FS_GLOBAL_OTG_FS_HPTXFSIZ.  RW   $" OTG_FS_GLOBAL_OTG_FS_HPTXFSIZ @ hex. OTG_FS_GLOBAL_OTG_FS_HPTXFSIZ 1b. ;
    : OTG_FS_GLOBAL_OTG_FS_DIEPTXF1. cr ." OTG_FS_GLOBAL_OTG_FS_DIEPTXF1.  RW   $" OTG_FS_GLOBAL_OTG_FS_DIEPTXF1 @ hex. OTG_FS_GLOBAL_OTG_FS_DIEPTXF1 1b. ;
    : OTG_FS_GLOBAL_OTG_FS_DIEPTXF2. cr ." OTG_FS_GLOBAL_OTG_FS_DIEPTXF2.  RW   $" OTG_FS_GLOBAL_OTG_FS_DIEPTXF2 @ hex. OTG_FS_GLOBAL_OTG_FS_DIEPTXF2 1b. ;
    : OTG_FS_GLOBAL_OTG_FS_DIEPTXF3. cr ." OTG_FS_GLOBAL_OTG_FS_DIEPTXF3.  RW   $" OTG_FS_GLOBAL_OTG_FS_DIEPTXF3 @ hex. OTG_FS_GLOBAL_OTG_FS_DIEPTXF3 1b. ;
    : OTG_FS_GLOBAL_OTG_FS_GRXSTSP_Device. cr ." OTG_FS_GLOBAL_OTG_FS_GRXSTSP_Device.  RO   $" OTG_FS_GLOBAL_OTG_FS_GRXSTSP_Device @ hex. OTG_FS_GLOBAL_OTG_FS_GRXSTSP_Device 1b. ;
    : OTG_FS_GLOBAL_OTG_FS_GRXSTSP_Host. cr ." OTG_FS_GLOBAL_OTG_FS_GRXSTSP_Host.  RO   $" OTG_FS_GLOBAL_OTG_FS_GRXSTSP_Host @ hex. OTG_FS_GLOBAL_OTG_FS_GRXSTSP_Host 1b. ;
    : OTG_FS_GLOBAL_OTG_FS_GI2CCTL. cr ." OTG_FS_GLOBAL_OTG_FS_GI2CCTL.  RW   $" OTG_FS_GLOBAL_OTG_FS_GI2CCTL @ hex. OTG_FS_GLOBAL_OTG_FS_GI2CCTL 1b. ;
    : OTG_FS_GLOBAL_OTG_FS_GPWRDN. cr ." OTG_FS_GLOBAL_OTG_FS_GPWRDN.  RW   $" OTG_FS_GLOBAL_OTG_FS_GPWRDN @ hex. OTG_FS_GLOBAL_OTG_FS_GPWRDN 1b. ;
    : OTG_FS_GLOBAL_OTG_FS_GADPCTL. cr ." OTG_FS_GLOBAL_OTG_FS_GADPCTL.   $" OTG_FS_GLOBAL_OTG_FS_GADPCTL @ hex. OTG_FS_GLOBAL_OTG_FS_GADPCTL 1b. ;
    : OTG_FS_GLOBAL_OTG_FS_DIEPTXF4. cr ." OTG_FS_GLOBAL_OTG_FS_DIEPTXF4.  RW   $" OTG_FS_GLOBAL_OTG_FS_DIEPTXF4 @ hex. OTG_FS_GLOBAL_OTG_FS_DIEPTXF4 1b. ;
    : OTG_FS_GLOBAL_OTG_FS_DIEPTXF5. cr ." OTG_FS_GLOBAL_OTG_FS_DIEPTXF5.  RW   $" OTG_FS_GLOBAL_OTG_FS_DIEPTXF5 @ hex. OTG_FS_GLOBAL_OTG_FS_DIEPTXF5 1b. ;
    : OTG_FS_GLOBAL_OTG_FS_GLPMCFG. cr ." OTG_FS_GLOBAL_OTG_FS_GLPMCFG.   $" OTG_FS_GLOBAL_OTG_FS_GLPMCFG @ hex. OTG_FS_GLOBAL_OTG_FS_GLPMCFG 1b. ;
    : OTG_FS_GLOBAL.
      OTG_FS_GLOBAL_OTG_FS_GOTGCTL.
      OTG_FS_GLOBAL_OTG_FS_GOTGINT.
      OTG_FS_GLOBAL_OTG_FS_GAHBCFG.
      OTG_FS_GLOBAL_OTG_FS_GUSBCFG.
      OTG_FS_GLOBAL_OTG_FS_GRSTCTL.
      OTG_FS_GLOBAL_OTG_FS_GINTSTS.
      OTG_FS_GLOBAL_OTG_FS_GINTMSK.
      OTG_FS_GLOBAL_OTG_FS_GRXSTSR_Device.
      OTG_FS_GLOBAL_OTG_FS_GRXSTSR_Host.
      OTG_FS_GLOBAL_OTG_FS_GRXFSIZ.
      OTG_FS_GLOBAL_OTG_FS_DIEPTXF0_Device.
      OTG_FS_GLOBAL_OTG_FS_HNPTXFSIZ_Host.
      OTG_FS_GLOBAL_OTG_FS_HNPTXSTS.
      OTG_FS_GLOBAL_OTG_FS_GCCFG.
      OTG_FS_GLOBAL_OTG_FS_CID.
      OTG_FS_GLOBAL_OTG_FS_HPTXFSIZ.
      OTG_FS_GLOBAL_OTG_FS_DIEPTXF1.
      OTG_FS_GLOBAL_OTG_FS_DIEPTXF2.
      OTG_FS_GLOBAL_OTG_FS_DIEPTXF3.
      OTG_FS_GLOBAL_OTG_FS_GRXSTSP_Device.
      OTG_FS_GLOBAL_OTG_FS_GRXSTSP_Host.
      OTG_FS_GLOBAL_OTG_FS_GI2CCTL.
      OTG_FS_GLOBAL_OTG_FS_GPWRDN.
      OTG_FS_GLOBAL_OTG_FS_GADPCTL.
      OTG_FS_GLOBAL_OTG_FS_DIEPTXF4.
      OTG_FS_GLOBAL_OTG_FS_DIEPTXF5.
      OTG_FS_GLOBAL_OTG_FS_GLPMCFG.
    ;
  [then]

  execute-defined? use-OTG_FS_HOST [if]
    $50000400 constant OTG_FS_HOST ( USB on the go full speed ) 
    OTG_FS_HOST $0 + constant OTG_FS_HOST_OTG_FS_HCFG (  )  \ OTG_FS host configuration register OTG_FS_HCFG
    OTG_FS_HOST $4 + constant OTG_FS_HOST_OTG_FS_HFIR ( read-write )  \ OTG_FS Host frame interval register
    OTG_FS_HOST $8 + constant OTG_FS_HOST_OTG_FS_HFNUM ( read-only )  \ OTG_FS host frame number/frame time remaining register OTG_FS_HFNUM
    OTG_FS_HOST $10 + constant OTG_FS_HOST_OTG_FS_HPTXSTS (  )  \ OTG_FS_Host periodic transmit FIFO/queue status register OTG_FS_HPTXSTS
    OTG_FS_HOST $14 + constant OTG_FS_HOST_OTG_FS_HAINT ( read-only )  \ OTG_FS Host all channels interrupt register
    OTG_FS_HOST $18 + constant OTG_FS_HOST_OTG_FS_HAINTMSK ( read-write )  \ OTG_FS host all channels interrupt mask register
    OTG_FS_HOST $40 + constant OTG_FS_HOST_OTG_FS_HPRT (  )  \ OTG_FS host port control and status register OTG_FS_HPRT
    OTG_FS_HOST $100 + constant OTG_FS_HOST_OTG_FS_HCCHAR0 ( read-write )  \ OTG_FS host channel-0 characteristics register OTG_FS_HCCHAR0
    OTG_FS_HOST $120 + constant OTG_FS_HOST_OTG_FS_HCCHAR1 ( read-write )  \ OTG_FS host channel-1 characteristics register OTG_FS_HCCHAR1
    OTG_FS_HOST $140 + constant OTG_FS_HOST_OTG_FS_HCCHAR2 ( read-write )  \ OTG_FS host channel-2 characteristics register OTG_FS_HCCHAR2
    OTG_FS_HOST $160 + constant OTG_FS_HOST_OTG_FS_HCCHAR3 ( read-write )  \ OTG_FS host channel-3 characteristics register OTG_FS_HCCHAR3
    OTG_FS_HOST $180 + constant OTG_FS_HOST_OTG_FS_HCCHAR4 ( read-write )  \ OTG_FS host channel-4 characteristics register OTG_FS_HCCHAR4
    OTG_FS_HOST $1A0 + constant OTG_FS_HOST_OTG_FS_HCCHAR5 ( read-write )  \ OTG_FS host channel-5 characteristics register OTG_FS_HCCHAR5
    OTG_FS_HOST $1C0 + constant OTG_FS_HOST_OTG_FS_HCCHAR6 ( read-write )  \ OTG_FS host channel-6 characteristics register OTG_FS_HCCHAR6
    OTG_FS_HOST $1E0 + constant OTG_FS_HOST_OTG_FS_HCCHAR7 ( read-write )  \ OTG_FS host channel-7 characteristics register OTG_FS_HCCHAR7
    OTG_FS_HOST $108 + constant OTG_FS_HOST_OTG_FS_HCINT0 ( read-write )  \ OTG_FS host channel-0 interrupt register OTG_FS_HCINT0
    OTG_FS_HOST $128 + constant OTG_FS_HOST_OTG_FS_HCINT1 ( read-write )  \ OTG_FS host channel-1 interrupt register OTG_FS_HCINT1
    OTG_FS_HOST $148 + constant OTG_FS_HOST_OTG_FS_HCINT2 ( read-write )  \ OTG_FS host channel-2 interrupt register OTG_FS_HCINT2
    OTG_FS_HOST $168 + constant OTG_FS_HOST_OTG_FS_HCINT3 ( read-write )  \ OTG_FS host channel-3 interrupt register OTG_FS_HCINT3
    OTG_FS_HOST $188 + constant OTG_FS_HOST_OTG_FS_HCINT4 ( read-write )  \ OTG_FS host channel-4 interrupt register OTG_FS_HCINT4
    OTG_FS_HOST $1A8 + constant OTG_FS_HOST_OTG_FS_HCINT5 ( read-write )  \ OTG_FS host channel-5 interrupt register OTG_FS_HCINT5
    OTG_FS_HOST $1C8 + constant OTG_FS_HOST_OTG_FS_HCINT6 ( read-write )  \ OTG_FS host channel-6 interrupt register OTG_FS_HCINT6
    OTG_FS_HOST $1E8 + constant OTG_FS_HOST_OTG_FS_HCINT7 ( read-write )  \ OTG_FS host channel-7 interrupt register OTG_FS_HCINT7
    OTG_FS_HOST $10C + constant OTG_FS_HOST_OTG_FS_HCINTMSK0 ( read-write )  \ OTG_FS host channel-0 mask register OTG_FS_HCINTMSK0
    OTG_FS_HOST $12C + constant OTG_FS_HOST_OTG_FS_HCINTMSK1 ( read-write )  \ OTG_FS host channel-1 mask register OTG_FS_HCINTMSK1
    OTG_FS_HOST $14C + constant OTG_FS_HOST_OTG_FS_HCINTMSK2 ( read-write )  \ OTG_FS host channel-2 mask register OTG_FS_HCINTMSK2
    OTG_FS_HOST $16C + constant OTG_FS_HOST_OTG_FS_HCINTMSK3 ( read-write )  \ OTG_FS host channel-3 mask register OTG_FS_HCINTMSK3
    OTG_FS_HOST $18C + constant OTG_FS_HOST_OTG_FS_HCINTMSK4 ( read-write )  \ OTG_FS host channel-4 mask register OTG_FS_HCINTMSK4
    OTG_FS_HOST $1AC + constant OTG_FS_HOST_OTG_FS_HCINTMSK5 ( read-write )  \ OTG_FS host channel-5 mask register OTG_FS_HCINTMSK5
    OTG_FS_HOST $1CC + constant OTG_FS_HOST_OTG_FS_HCINTMSK6 ( read-write )  \ OTG_FS host channel-6 mask register OTG_FS_HCINTMSK6
    OTG_FS_HOST $1EC + constant OTG_FS_HOST_OTG_FS_HCINTMSK7 ( read-write )  \ OTG_FS host channel-7 mask register OTG_FS_HCINTMSK7
    OTG_FS_HOST $110 + constant OTG_FS_HOST_OTG_FS_HCTSIZ0 ( read-write )  \ OTG_FS host channel-0 transfer size register
    OTG_FS_HOST $130 + constant OTG_FS_HOST_OTG_FS_HCTSIZ1 ( read-write )  \ OTG_FS host channel-1 transfer size register
    OTG_FS_HOST $150 + constant OTG_FS_HOST_OTG_FS_HCTSIZ2 ( read-write )  \ OTG_FS host channel-2 transfer size register
    OTG_FS_HOST $170 + constant OTG_FS_HOST_OTG_FS_HCTSIZ3 ( read-write )  \ OTG_FS host channel-3 transfer size register
    OTG_FS_HOST $190 + constant OTG_FS_HOST_OTG_FS_HCTSIZ4 ( read-write )  \ OTG_FS host channel-x transfer size register
    OTG_FS_HOST $1B0 + constant OTG_FS_HOST_OTG_FS_HCTSIZ5 ( read-write )  \ OTG_FS host channel-5 transfer size register
    OTG_FS_HOST $1D0 + constant OTG_FS_HOST_OTG_FS_HCTSIZ6 ( read-write )  \ OTG_FS host channel-6 transfer size register
    OTG_FS_HOST $1F0 + constant OTG_FS_HOST_OTG_FS_HCTSIZ7 ( read-write )  \ OTG_FS host channel-7 transfer size register
    OTG_FS_HOST $1F4 + constant OTG_FS_HOST_OTG_FS_HCCHAR8 ( read-write )  \ OTG_FS host channel-8 characteristics register
    OTG_FS_HOST $1F8 + constant OTG_FS_HOST_OTG_FS_HCINT8 ( read-write )  \ OTG_FS host channel-8 interrupt register
    OTG_FS_HOST $1FC + constant OTG_FS_HOST_OTG_FS_HCINTMSK8 ( read-write )  \ OTG_FS host channel-8 mask register
    OTG_FS_HOST $200 + constant OTG_FS_HOST_OTG_FS_HCTSIZ8 ( read-write )  \ OTG_FS host channel-8 transfer size register
    OTG_FS_HOST $204 + constant OTG_FS_HOST_OTG_FS_HCCHAR9 ( read-write )  \ OTG_FS host channel-9 characteristics register
    OTG_FS_HOST $208 + constant OTG_FS_HOST_OTG_FS_HCINT9 ( read-write )  \ OTG_FS host channel-9 interrupt register
    OTG_FS_HOST $20C + constant OTG_FS_HOST_OTG_FS_HCINTMSK9 ( read-write )  \ OTG_FS host channel-9 mask register
    OTG_FS_HOST $210 + constant OTG_FS_HOST_OTG_FS_HCTSIZ9 ( read-write )  \ OTG_FS host channel-9 transfer size register
    OTG_FS_HOST $214 + constant OTG_FS_HOST_OTG_FS_HCCHAR10 ( read-write )  \ OTG_FS host channel-10 characteristics register
    OTG_FS_HOST $218 + constant OTG_FS_HOST_OTG_FS_HCINT10 ( read-write )  \ OTG_FS host channel-10 interrupt register
    OTG_FS_HOST $21C + constant OTG_FS_HOST_OTG_FS_HCINTMSK10 ( read-write )  \ OTG_FS host channel-10 mask register
    OTG_FS_HOST $220 + constant OTG_FS_HOST_OTG_FS_HCTSIZ10 ( read-write )  \ OTG_FS host channel-10 transfer size register
    OTG_FS_HOST $224 + constant OTG_FS_HOST_OTG_FS_HCCHAR11 ( read-write )  \ OTG_FS host channel-11 characteristics register
    OTG_FS_HOST $228 + constant OTG_FS_HOST_OTG_FS_HCINT11 ( read-write )  \ OTG_FS host channel-11 interrupt register
    OTG_FS_HOST $22C + constant OTG_FS_HOST_OTG_FS_HCINTMSK11 ( read-write )  \ OTG_FS host channel-11 mask register
    OTG_FS_HOST $230 + constant OTG_FS_HOST_OTG_FS_HCTSIZ11 ( read-write )  \ OTG_FS host channel-11 transfer size register
    : OTG_FS_HOST_OTG_FS_HCFG. cr ." OTG_FS_HOST_OTG_FS_HCFG.   $" OTG_FS_HOST_OTG_FS_HCFG @ hex. OTG_FS_HOST_OTG_FS_HCFG 1b. ;
    : OTG_FS_HOST_OTG_FS_HFIR. cr ." OTG_FS_HOST_OTG_FS_HFIR.  RW   $" OTG_FS_HOST_OTG_FS_HFIR @ hex. OTG_FS_HOST_OTG_FS_HFIR 1b. ;
    : OTG_FS_HOST_OTG_FS_HFNUM. cr ." OTG_FS_HOST_OTG_FS_HFNUM.  RO   $" OTG_FS_HOST_OTG_FS_HFNUM @ hex. OTG_FS_HOST_OTG_FS_HFNUM 1b. ;
    : OTG_FS_HOST_OTG_FS_HPTXSTS. cr ." OTG_FS_HOST_OTG_FS_HPTXSTS.   $" OTG_FS_HOST_OTG_FS_HPTXSTS @ hex. OTG_FS_HOST_OTG_FS_HPTXSTS 1b. ;
    : OTG_FS_HOST_OTG_FS_HAINT. cr ." OTG_FS_HOST_OTG_FS_HAINT.  RO   $" OTG_FS_HOST_OTG_FS_HAINT @ hex. OTG_FS_HOST_OTG_FS_HAINT 1b. ;
    : OTG_FS_HOST_OTG_FS_HAINTMSK. cr ." OTG_FS_HOST_OTG_FS_HAINTMSK.  RW   $" OTG_FS_HOST_OTG_FS_HAINTMSK @ hex. OTG_FS_HOST_OTG_FS_HAINTMSK 1b. ;
    : OTG_FS_HOST_OTG_FS_HPRT. cr ." OTG_FS_HOST_OTG_FS_HPRT.   $" OTG_FS_HOST_OTG_FS_HPRT @ hex. OTG_FS_HOST_OTG_FS_HPRT 1b. ;
    : OTG_FS_HOST_OTG_FS_HCCHAR0. cr ." OTG_FS_HOST_OTG_FS_HCCHAR0.  RW   $" OTG_FS_HOST_OTG_FS_HCCHAR0 @ hex. OTG_FS_HOST_OTG_FS_HCCHAR0 1b. ;
    : OTG_FS_HOST_OTG_FS_HCCHAR1. cr ." OTG_FS_HOST_OTG_FS_HCCHAR1.  RW   $" OTG_FS_HOST_OTG_FS_HCCHAR1 @ hex. OTG_FS_HOST_OTG_FS_HCCHAR1 1b. ;
    : OTG_FS_HOST_OTG_FS_HCCHAR2. cr ." OTG_FS_HOST_OTG_FS_HCCHAR2.  RW   $" OTG_FS_HOST_OTG_FS_HCCHAR2 @ hex. OTG_FS_HOST_OTG_FS_HCCHAR2 1b. ;
    : OTG_FS_HOST_OTG_FS_HCCHAR3. cr ." OTG_FS_HOST_OTG_FS_HCCHAR3.  RW   $" OTG_FS_HOST_OTG_FS_HCCHAR3 @ hex. OTG_FS_HOST_OTG_FS_HCCHAR3 1b. ;
    : OTG_FS_HOST_OTG_FS_HCCHAR4. cr ." OTG_FS_HOST_OTG_FS_HCCHAR4.  RW   $" OTG_FS_HOST_OTG_FS_HCCHAR4 @ hex. OTG_FS_HOST_OTG_FS_HCCHAR4 1b. ;
    : OTG_FS_HOST_OTG_FS_HCCHAR5. cr ." OTG_FS_HOST_OTG_FS_HCCHAR5.  RW   $" OTG_FS_HOST_OTG_FS_HCCHAR5 @ hex. OTG_FS_HOST_OTG_FS_HCCHAR5 1b. ;
    : OTG_FS_HOST_OTG_FS_HCCHAR6. cr ." OTG_FS_HOST_OTG_FS_HCCHAR6.  RW   $" OTG_FS_HOST_OTG_FS_HCCHAR6 @ hex. OTG_FS_HOST_OTG_FS_HCCHAR6 1b. ;
    : OTG_FS_HOST_OTG_FS_HCCHAR7. cr ." OTG_FS_HOST_OTG_FS_HCCHAR7.  RW   $" OTG_FS_HOST_OTG_FS_HCCHAR7 @ hex. OTG_FS_HOST_OTG_FS_HCCHAR7 1b. ;
    : OTG_FS_HOST_OTG_FS_HCINT0. cr ." OTG_FS_HOST_OTG_FS_HCINT0.  RW   $" OTG_FS_HOST_OTG_FS_HCINT0 @ hex. OTG_FS_HOST_OTG_FS_HCINT0 1b. ;
    : OTG_FS_HOST_OTG_FS_HCINT1. cr ." OTG_FS_HOST_OTG_FS_HCINT1.  RW   $" OTG_FS_HOST_OTG_FS_HCINT1 @ hex. OTG_FS_HOST_OTG_FS_HCINT1 1b. ;
    : OTG_FS_HOST_OTG_FS_HCINT2. cr ." OTG_FS_HOST_OTG_FS_HCINT2.  RW   $" OTG_FS_HOST_OTG_FS_HCINT2 @ hex. OTG_FS_HOST_OTG_FS_HCINT2 1b. ;
    : OTG_FS_HOST_OTG_FS_HCINT3. cr ." OTG_FS_HOST_OTG_FS_HCINT3.  RW   $" OTG_FS_HOST_OTG_FS_HCINT3 @ hex. OTG_FS_HOST_OTG_FS_HCINT3 1b. ;
    : OTG_FS_HOST_OTG_FS_HCINT4. cr ." OTG_FS_HOST_OTG_FS_HCINT4.  RW   $" OTG_FS_HOST_OTG_FS_HCINT4 @ hex. OTG_FS_HOST_OTG_FS_HCINT4 1b. ;
    : OTG_FS_HOST_OTG_FS_HCINT5. cr ." OTG_FS_HOST_OTG_FS_HCINT5.  RW   $" OTG_FS_HOST_OTG_FS_HCINT5 @ hex. OTG_FS_HOST_OTG_FS_HCINT5 1b. ;
    : OTG_FS_HOST_OTG_FS_HCINT6. cr ." OTG_FS_HOST_OTG_FS_HCINT6.  RW   $" OTG_FS_HOST_OTG_FS_HCINT6 @ hex. OTG_FS_HOST_OTG_FS_HCINT6 1b. ;
    : OTG_FS_HOST_OTG_FS_HCINT7. cr ." OTG_FS_HOST_OTG_FS_HCINT7.  RW   $" OTG_FS_HOST_OTG_FS_HCINT7 @ hex. OTG_FS_HOST_OTG_FS_HCINT7 1b. ;
    : OTG_FS_HOST_OTG_FS_HCINTMSK0. cr ." OTG_FS_HOST_OTG_FS_HCINTMSK0.  RW   $" OTG_FS_HOST_OTG_FS_HCINTMSK0 @ hex. OTG_FS_HOST_OTG_FS_HCINTMSK0 1b. ;
    : OTG_FS_HOST_OTG_FS_HCINTMSK1. cr ." OTG_FS_HOST_OTG_FS_HCINTMSK1.  RW   $" OTG_FS_HOST_OTG_FS_HCINTMSK1 @ hex. OTG_FS_HOST_OTG_FS_HCINTMSK1 1b. ;
    : OTG_FS_HOST_OTG_FS_HCINTMSK2. cr ." OTG_FS_HOST_OTG_FS_HCINTMSK2.  RW   $" OTG_FS_HOST_OTG_FS_HCINTMSK2 @ hex. OTG_FS_HOST_OTG_FS_HCINTMSK2 1b. ;
    : OTG_FS_HOST_OTG_FS_HCINTMSK3. cr ." OTG_FS_HOST_OTG_FS_HCINTMSK3.  RW   $" OTG_FS_HOST_OTG_FS_HCINTMSK3 @ hex. OTG_FS_HOST_OTG_FS_HCINTMSK3 1b. ;
    : OTG_FS_HOST_OTG_FS_HCINTMSK4. cr ." OTG_FS_HOST_OTG_FS_HCINTMSK4.  RW   $" OTG_FS_HOST_OTG_FS_HCINTMSK4 @ hex. OTG_FS_HOST_OTG_FS_HCINTMSK4 1b. ;
    : OTG_FS_HOST_OTG_FS_HCINTMSK5. cr ." OTG_FS_HOST_OTG_FS_HCINTMSK5.  RW   $" OTG_FS_HOST_OTG_FS_HCINTMSK5 @ hex. OTG_FS_HOST_OTG_FS_HCINTMSK5 1b. ;
    : OTG_FS_HOST_OTG_FS_HCINTMSK6. cr ." OTG_FS_HOST_OTG_FS_HCINTMSK6.  RW   $" OTG_FS_HOST_OTG_FS_HCINTMSK6 @ hex. OTG_FS_HOST_OTG_FS_HCINTMSK6 1b. ;
    : OTG_FS_HOST_OTG_FS_HCINTMSK7. cr ." OTG_FS_HOST_OTG_FS_HCINTMSK7.  RW   $" OTG_FS_HOST_OTG_FS_HCINTMSK7 @ hex. OTG_FS_HOST_OTG_FS_HCINTMSK7 1b. ;
    : OTG_FS_HOST_OTG_FS_HCTSIZ0. cr ." OTG_FS_HOST_OTG_FS_HCTSIZ0.  RW   $" OTG_FS_HOST_OTG_FS_HCTSIZ0 @ hex. OTG_FS_HOST_OTG_FS_HCTSIZ0 1b. ;
    : OTG_FS_HOST_OTG_FS_HCTSIZ1. cr ." OTG_FS_HOST_OTG_FS_HCTSIZ1.  RW   $" OTG_FS_HOST_OTG_FS_HCTSIZ1 @ hex. OTG_FS_HOST_OTG_FS_HCTSIZ1 1b. ;
    : OTG_FS_HOST_OTG_FS_HCTSIZ2. cr ." OTG_FS_HOST_OTG_FS_HCTSIZ2.  RW   $" OTG_FS_HOST_OTG_FS_HCTSIZ2 @ hex. OTG_FS_HOST_OTG_FS_HCTSIZ2 1b. ;
    : OTG_FS_HOST_OTG_FS_HCTSIZ3. cr ." OTG_FS_HOST_OTG_FS_HCTSIZ3.  RW   $" OTG_FS_HOST_OTG_FS_HCTSIZ3 @ hex. OTG_FS_HOST_OTG_FS_HCTSIZ3 1b. ;
    : OTG_FS_HOST_OTG_FS_HCTSIZ4. cr ." OTG_FS_HOST_OTG_FS_HCTSIZ4.  RW   $" OTG_FS_HOST_OTG_FS_HCTSIZ4 @ hex. OTG_FS_HOST_OTG_FS_HCTSIZ4 1b. ;
    : OTG_FS_HOST_OTG_FS_HCTSIZ5. cr ." OTG_FS_HOST_OTG_FS_HCTSIZ5.  RW   $" OTG_FS_HOST_OTG_FS_HCTSIZ5 @ hex. OTG_FS_HOST_OTG_FS_HCTSIZ5 1b. ;
    : OTG_FS_HOST_OTG_FS_HCTSIZ6. cr ." OTG_FS_HOST_OTG_FS_HCTSIZ6.  RW   $" OTG_FS_HOST_OTG_FS_HCTSIZ6 @ hex. OTG_FS_HOST_OTG_FS_HCTSIZ6 1b. ;
    : OTG_FS_HOST_OTG_FS_HCTSIZ7. cr ." OTG_FS_HOST_OTG_FS_HCTSIZ7.  RW   $" OTG_FS_HOST_OTG_FS_HCTSIZ7 @ hex. OTG_FS_HOST_OTG_FS_HCTSIZ7 1b. ;
    : OTG_FS_HOST_OTG_FS_HCCHAR8. cr ." OTG_FS_HOST_OTG_FS_HCCHAR8.  RW   $" OTG_FS_HOST_OTG_FS_HCCHAR8 @ hex. OTG_FS_HOST_OTG_FS_HCCHAR8 1b. ;
    : OTG_FS_HOST_OTG_FS_HCINT8. cr ." OTG_FS_HOST_OTG_FS_HCINT8.  RW   $" OTG_FS_HOST_OTG_FS_HCINT8 @ hex. OTG_FS_HOST_OTG_FS_HCINT8 1b. ;
    : OTG_FS_HOST_OTG_FS_HCINTMSK8. cr ." OTG_FS_HOST_OTG_FS_HCINTMSK8.  RW   $" OTG_FS_HOST_OTG_FS_HCINTMSK8 @ hex. OTG_FS_HOST_OTG_FS_HCINTMSK8 1b. ;
    : OTG_FS_HOST_OTG_FS_HCTSIZ8. cr ." OTG_FS_HOST_OTG_FS_HCTSIZ8.  RW   $" OTG_FS_HOST_OTG_FS_HCTSIZ8 @ hex. OTG_FS_HOST_OTG_FS_HCTSIZ8 1b. ;
    : OTG_FS_HOST_OTG_FS_HCCHAR9. cr ." OTG_FS_HOST_OTG_FS_HCCHAR9.  RW   $" OTG_FS_HOST_OTG_FS_HCCHAR9 @ hex. OTG_FS_HOST_OTG_FS_HCCHAR9 1b. ;
    : OTG_FS_HOST_OTG_FS_HCINT9. cr ." OTG_FS_HOST_OTG_FS_HCINT9.  RW   $" OTG_FS_HOST_OTG_FS_HCINT9 @ hex. OTG_FS_HOST_OTG_FS_HCINT9 1b. ;
    : OTG_FS_HOST_OTG_FS_HCINTMSK9. cr ." OTG_FS_HOST_OTG_FS_HCINTMSK9.  RW   $" OTG_FS_HOST_OTG_FS_HCINTMSK9 @ hex. OTG_FS_HOST_OTG_FS_HCINTMSK9 1b. ;
    : OTG_FS_HOST_OTG_FS_HCTSIZ9. cr ." OTG_FS_HOST_OTG_FS_HCTSIZ9.  RW   $" OTG_FS_HOST_OTG_FS_HCTSIZ9 @ hex. OTG_FS_HOST_OTG_FS_HCTSIZ9 1b. ;
    : OTG_FS_HOST_OTG_FS_HCCHAR10. cr ." OTG_FS_HOST_OTG_FS_HCCHAR10.  RW   $" OTG_FS_HOST_OTG_FS_HCCHAR10 @ hex. OTG_FS_HOST_OTG_FS_HCCHAR10 1b. ;
    : OTG_FS_HOST_OTG_FS_HCINT10. cr ." OTG_FS_HOST_OTG_FS_HCINT10.  RW   $" OTG_FS_HOST_OTG_FS_HCINT10 @ hex. OTG_FS_HOST_OTG_FS_HCINT10 1b. ;
    : OTG_FS_HOST_OTG_FS_HCINTMSK10. cr ." OTG_FS_HOST_OTG_FS_HCINTMSK10.  RW   $" OTG_FS_HOST_OTG_FS_HCINTMSK10 @ hex. OTG_FS_HOST_OTG_FS_HCINTMSK10 1b. ;
    : OTG_FS_HOST_OTG_FS_HCTSIZ10. cr ." OTG_FS_HOST_OTG_FS_HCTSIZ10.  RW   $" OTG_FS_HOST_OTG_FS_HCTSIZ10 @ hex. OTG_FS_HOST_OTG_FS_HCTSIZ10 1b. ;
    : OTG_FS_HOST_OTG_FS_HCCHAR11. cr ." OTG_FS_HOST_OTG_FS_HCCHAR11.  RW   $" OTG_FS_HOST_OTG_FS_HCCHAR11 @ hex. OTG_FS_HOST_OTG_FS_HCCHAR11 1b. ;
    : OTG_FS_HOST_OTG_FS_HCINT11. cr ." OTG_FS_HOST_OTG_FS_HCINT11.  RW   $" OTG_FS_HOST_OTG_FS_HCINT11 @ hex. OTG_FS_HOST_OTG_FS_HCINT11 1b. ;
    : OTG_FS_HOST_OTG_FS_HCINTMSK11. cr ." OTG_FS_HOST_OTG_FS_HCINTMSK11.  RW   $" OTG_FS_HOST_OTG_FS_HCINTMSK11 @ hex. OTG_FS_HOST_OTG_FS_HCINTMSK11 1b. ;
    : OTG_FS_HOST_OTG_FS_HCTSIZ11. cr ." OTG_FS_HOST_OTG_FS_HCTSIZ11.  RW   $" OTG_FS_HOST_OTG_FS_HCTSIZ11 @ hex. OTG_FS_HOST_OTG_FS_HCTSIZ11 1b. ;
    : OTG_FS_HOST.
      OTG_FS_HOST_OTG_FS_HCFG.
      OTG_FS_HOST_OTG_FS_HFIR.
      OTG_FS_HOST_OTG_FS_HFNUM.
      OTG_FS_HOST_OTG_FS_HPTXSTS.
      OTG_FS_HOST_OTG_FS_HAINT.
      OTG_FS_HOST_OTG_FS_HAINTMSK.
      OTG_FS_HOST_OTG_FS_HPRT.
      OTG_FS_HOST_OTG_FS_HCCHAR0.
      OTG_FS_HOST_OTG_FS_HCCHAR1.
      OTG_FS_HOST_OTG_FS_HCCHAR2.
      OTG_FS_HOST_OTG_FS_HCCHAR3.
      OTG_FS_HOST_OTG_FS_HCCHAR4.
      OTG_FS_HOST_OTG_FS_HCCHAR5.
      OTG_FS_HOST_OTG_FS_HCCHAR6.
      OTG_FS_HOST_OTG_FS_HCCHAR7.
      OTG_FS_HOST_OTG_FS_HCINT0.
      OTG_FS_HOST_OTG_FS_HCINT1.
      OTG_FS_HOST_OTG_FS_HCINT2.
      OTG_FS_HOST_OTG_FS_HCINT3.
      OTG_FS_HOST_OTG_FS_HCINT4.
      OTG_FS_HOST_OTG_FS_HCINT5.
      OTG_FS_HOST_OTG_FS_HCINT6.
      OTG_FS_HOST_OTG_FS_HCINT7.
      OTG_FS_HOST_OTG_FS_HCINTMSK0.
      OTG_FS_HOST_OTG_FS_HCINTMSK1.
      OTG_FS_HOST_OTG_FS_HCINTMSK2.
      OTG_FS_HOST_OTG_FS_HCINTMSK3.
      OTG_FS_HOST_OTG_FS_HCINTMSK4.
      OTG_FS_HOST_OTG_FS_HCINTMSK5.
      OTG_FS_HOST_OTG_FS_HCINTMSK6.
      OTG_FS_HOST_OTG_FS_HCINTMSK7.
      OTG_FS_HOST_OTG_FS_HCTSIZ0.
      OTG_FS_HOST_OTG_FS_HCTSIZ1.
      OTG_FS_HOST_OTG_FS_HCTSIZ2.
      OTG_FS_HOST_OTG_FS_HCTSIZ3.
      OTG_FS_HOST_OTG_FS_HCTSIZ4.
      OTG_FS_HOST_OTG_FS_HCTSIZ5.
      OTG_FS_HOST_OTG_FS_HCTSIZ6.
      OTG_FS_HOST_OTG_FS_HCTSIZ7.
      OTG_FS_HOST_OTG_FS_HCCHAR8.
      OTG_FS_HOST_OTG_FS_HCINT8.
      OTG_FS_HOST_OTG_FS_HCINTMSK8.
      OTG_FS_HOST_OTG_FS_HCTSIZ8.
      OTG_FS_HOST_OTG_FS_HCCHAR9.
      OTG_FS_HOST_OTG_FS_HCINT9.
      OTG_FS_HOST_OTG_FS_HCINTMSK9.
      OTG_FS_HOST_OTG_FS_HCTSIZ9.
      OTG_FS_HOST_OTG_FS_HCCHAR10.
      OTG_FS_HOST_OTG_FS_HCINT10.
      OTG_FS_HOST_OTG_FS_HCINTMSK10.
      OTG_FS_HOST_OTG_FS_HCTSIZ10.
      OTG_FS_HOST_OTG_FS_HCCHAR11.
      OTG_FS_HOST_OTG_FS_HCINT11.
      OTG_FS_HOST_OTG_FS_HCINTMSK11.
      OTG_FS_HOST_OTG_FS_HCTSIZ11.
    ;
  [then]

  execute-defined? use-OTG_FS_DEVICE [if]
    $50000800 constant OTG_FS_DEVICE ( USB on the go full speed ) 
    OTG_FS_DEVICE $0 + constant OTG_FS_DEVICE_OTG_FS_DCFG ( read-write )  \ OTG_FS device configuration register OTG_FS_DCFG
    OTG_FS_DEVICE $4 + constant OTG_FS_DEVICE_OTG_FS_DCTL (  )  \ OTG_FS device control register OTG_FS_DCTL
    OTG_FS_DEVICE $8 + constant OTG_FS_DEVICE_OTG_FS_DSTS ( read-only )  \ OTG_FS device status register OTG_FS_DSTS
    OTG_FS_DEVICE $10 + constant OTG_FS_DEVICE_OTG_FS_DIEPMSK ( read-write )  \ OTG_FS device IN endpoint common interrupt mask register OTG_FS_DIEPMSK
    OTG_FS_DEVICE $14 + constant OTG_FS_DEVICE_OTG_FS_DOEPMSK ( read-write )  \ OTG_FS device OUT endpoint common interrupt mask register OTG_FS_DOEPMSK
    OTG_FS_DEVICE $18 + constant OTG_FS_DEVICE_OTG_FS_DAINT ( read-only )  \ OTG_FS device all endpoints interrupt register OTG_FS_DAINT
    OTG_FS_DEVICE $1C + constant OTG_FS_DEVICE_OTG_FS_DAINTMSK ( read-write )  \ OTG_FS all endpoints interrupt mask register OTG_FS_DAINTMSK
    OTG_FS_DEVICE $28 + constant OTG_FS_DEVICE_OTG_FS_DVBUSDIS ( read-write )  \ OTG_FS device VBUS discharge time register
    OTG_FS_DEVICE $2C + constant OTG_FS_DEVICE_OTG_FS_DVBUSPULSE ( read-write )  \ OTG_FS device VBUS pulsing time register
    OTG_FS_DEVICE $34 + constant OTG_FS_DEVICE_OTG_FS_DIEPEMPMSK ( read-write )  \ OTG_FS device IN endpoint FIFO empty interrupt mask register
    OTG_FS_DEVICE $100 + constant OTG_FS_DEVICE_OTG_FS_DIEPCTL0 (  )  \ OTG_FS device control IN endpoint 0 control register OTG_FS_DIEPCTL0
    OTG_FS_DEVICE $120 + constant OTG_FS_DEVICE_OTG_FS_DIEPCTL1 (  )  \ OTG device endpoint-1 control register
    OTG_FS_DEVICE $140 + constant OTG_FS_DEVICE_OTG_FS_DIEPCTL2 (  )  \ OTG device endpoint-2 control register
    OTG_FS_DEVICE $160 + constant OTG_FS_DEVICE_OTG_FS_DIEPCTL3 (  )  \ OTG device endpoint-3 control register
    OTG_FS_DEVICE $300 + constant OTG_FS_DEVICE_OTG_FS_DOEPCTL0 (  )  \ device endpoint-0 control register
    OTG_FS_DEVICE $320 + constant OTG_FS_DEVICE_OTG_FS_DOEPCTL1 (  )  \ device endpoint-1 control register
    OTG_FS_DEVICE $340 + constant OTG_FS_DEVICE_OTG_FS_DOEPCTL2 (  )  \ device endpoint-2 control register
    OTG_FS_DEVICE $360 + constant OTG_FS_DEVICE_OTG_FS_DOEPCTL3 (  )  \ device endpoint-3 control register
    OTG_FS_DEVICE $108 + constant OTG_FS_DEVICE_OTG_FS_DIEPINT0 (  )  \ device endpoint-x interrupt register
    OTG_FS_DEVICE $128 + constant OTG_FS_DEVICE_OTG_FS_DIEPINT1 (  )  \ device endpoint-1 interrupt register
    OTG_FS_DEVICE $148 + constant OTG_FS_DEVICE_OTG_FS_DIEPINT2 (  )  \ device endpoint-2 interrupt register
    OTG_FS_DEVICE $168 + constant OTG_FS_DEVICE_OTG_FS_DIEPINT3 (  )  \ device endpoint-3 interrupt register
    OTG_FS_DEVICE $308 + constant OTG_FS_DEVICE_OTG_FS_DOEPINT0 ( read-write )  \ device endpoint-0 interrupt register
    OTG_FS_DEVICE $328 + constant OTG_FS_DEVICE_OTG_FS_DOEPINT1 ( read-write )  \ device endpoint-1 interrupt register
    OTG_FS_DEVICE $348 + constant OTG_FS_DEVICE_OTG_FS_DOEPINT2 ( read-write )  \ device endpoint-2 interrupt register
    OTG_FS_DEVICE $368 + constant OTG_FS_DEVICE_OTG_FS_DOEPINT3 ( read-write )  \ device endpoint-3 interrupt register
    OTG_FS_DEVICE $110 + constant OTG_FS_DEVICE_OTG_FS_DIEPTSIZ0 ( read-write )  \ device endpoint-0 transfer size register
    OTG_FS_DEVICE $310 + constant OTG_FS_DEVICE_OTG_FS_DOEPTSIZ0 ( read-write )  \ device OUT endpoint-0 transfer size register
    OTG_FS_DEVICE $130 + constant OTG_FS_DEVICE_OTG_FS_DIEPTSIZ1 ( read-write )  \ device endpoint-1 transfer size register
    OTG_FS_DEVICE $150 + constant OTG_FS_DEVICE_OTG_FS_DIEPTSIZ2 ( read-write )  \ device endpoint-2 transfer size register
    OTG_FS_DEVICE $170 + constant OTG_FS_DEVICE_OTG_FS_DIEPTSIZ3 ( read-write )  \ device endpoint-3 transfer size register
    OTG_FS_DEVICE $118 + constant OTG_FS_DEVICE_OTG_FS_DTXFSTS0 ( read-only )  \ OTG_FS device IN endpoint transmit FIFO status register
    OTG_FS_DEVICE $138 + constant OTG_FS_DEVICE_OTG_FS_DTXFSTS1 ( read-only )  \ OTG_FS device IN endpoint transmit FIFO status register
    OTG_FS_DEVICE $158 + constant OTG_FS_DEVICE_OTG_FS_DTXFSTS2 ( read-only )  \ OTG_FS device IN endpoint transmit FIFO status register
    OTG_FS_DEVICE $178 + constant OTG_FS_DEVICE_OTG_FS_DTXFSTS3 ( read-only )  \ OTG_FS device IN endpoint transmit FIFO status register
    OTG_FS_DEVICE $330 + constant OTG_FS_DEVICE_OTG_FS_DOEPTSIZ1 ( read-write )  \ device OUT endpoint-1 transfer size register
    OTG_FS_DEVICE $350 + constant OTG_FS_DEVICE_OTG_FS_DOEPTSIZ2 ( read-write )  \ device OUT endpoint-2 transfer size register
    OTG_FS_DEVICE $370 + constant OTG_FS_DEVICE_OTG_FS_DOEPTSIZ3 ( read-write )  \ device OUT endpoint-3 transfer size register
    OTG_FS_DEVICE $180 + constant OTG_FS_DEVICE_OTG_FS_DIEPCTL4 (  )  \ OTG device endpoint-4 control register
    OTG_FS_DEVICE $188 + constant OTG_FS_DEVICE_OTG_FS_DIEPINT4 (  )  \ device endpoint-4 interrupt register
    OTG_FS_DEVICE $194 + constant OTG_FS_DEVICE_OTG_FS_DIEPTSIZ4 ( read-write )  \ device endpoint-4 transfer size register
    OTG_FS_DEVICE $19C + constant OTG_FS_DEVICE_OTG_FS_DTXFSTS4 ( read-write )  \ OTG_FS device IN endpoint transmit FIFO status register
    OTG_FS_DEVICE $1A0 + constant OTG_FS_DEVICE_OTG_FS_DIEPCTL5 (  )  \ OTG device endpoint-5 control register
    OTG_FS_DEVICE $1A8 + constant OTG_FS_DEVICE_OTG_FS_DIEPINT5 (  )  \ device endpoint-5 interrupt register
    OTG_FS_DEVICE $1B0 + constant OTG_FS_DEVICE_OTG_FS_DIEPTSIZ55 ( read-write )  \ device endpoint-5 transfer size register
    OTG_FS_DEVICE $1B8 + constant OTG_FS_DEVICE_OTG_FS_DTXFSTS55 ( read-write )  \ OTG_FS device IN endpoint transmit FIFO status register
    OTG_FS_DEVICE $378 + constant OTG_FS_DEVICE_OTG_FS_DOEPCTL4 (  )  \ device endpoint-4 control register
    OTG_FS_DEVICE $380 + constant OTG_FS_DEVICE_OTG_FS_DOEPINT4 ( read-write )  \ device endpoint-4 interrupt register
    OTG_FS_DEVICE $388 + constant OTG_FS_DEVICE_OTG_FS_DOEPTSIZ4 ( read-write )  \ device OUT endpoint-4 transfer size register
    OTG_FS_DEVICE $390 + constant OTG_FS_DEVICE_OTG_FS_DOEPCTL5 (  )  \ device endpoint-5 control register
    OTG_FS_DEVICE $398 + constant OTG_FS_DEVICE_OTG_FS_DOEPINT5 ( read-write )  \ device endpoint-5 interrupt register
    OTG_FS_DEVICE $3A0 + constant OTG_FS_DEVICE_OTG_FS_DOEPTSIZ5 ( read-write )  \ device OUT endpoint-5 transfer size register
    : OTG_FS_DEVICE_OTG_FS_DCFG. cr ." OTG_FS_DEVICE_OTG_FS_DCFG.  RW   $" OTG_FS_DEVICE_OTG_FS_DCFG @ hex. OTG_FS_DEVICE_OTG_FS_DCFG 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DCTL. cr ." OTG_FS_DEVICE_OTG_FS_DCTL.   $" OTG_FS_DEVICE_OTG_FS_DCTL @ hex. OTG_FS_DEVICE_OTG_FS_DCTL 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DSTS. cr ." OTG_FS_DEVICE_OTG_FS_DSTS.  RO   $" OTG_FS_DEVICE_OTG_FS_DSTS @ hex. OTG_FS_DEVICE_OTG_FS_DSTS 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DIEPMSK. cr ." OTG_FS_DEVICE_OTG_FS_DIEPMSK.  RW   $" OTG_FS_DEVICE_OTG_FS_DIEPMSK @ hex. OTG_FS_DEVICE_OTG_FS_DIEPMSK 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DOEPMSK. cr ." OTG_FS_DEVICE_OTG_FS_DOEPMSK.  RW   $" OTG_FS_DEVICE_OTG_FS_DOEPMSK @ hex. OTG_FS_DEVICE_OTG_FS_DOEPMSK 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DAINT. cr ." OTG_FS_DEVICE_OTG_FS_DAINT.  RO   $" OTG_FS_DEVICE_OTG_FS_DAINT @ hex. OTG_FS_DEVICE_OTG_FS_DAINT 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DAINTMSK. cr ." OTG_FS_DEVICE_OTG_FS_DAINTMSK.  RW   $" OTG_FS_DEVICE_OTG_FS_DAINTMSK @ hex. OTG_FS_DEVICE_OTG_FS_DAINTMSK 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DVBUSDIS. cr ." OTG_FS_DEVICE_OTG_FS_DVBUSDIS.  RW   $" OTG_FS_DEVICE_OTG_FS_DVBUSDIS @ hex. OTG_FS_DEVICE_OTG_FS_DVBUSDIS 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DVBUSPULSE. cr ." OTG_FS_DEVICE_OTG_FS_DVBUSPULSE.  RW   $" OTG_FS_DEVICE_OTG_FS_DVBUSPULSE @ hex. OTG_FS_DEVICE_OTG_FS_DVBUSPULSE 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DIEPEMPMSK. cr ." OTG_FS_DEVICE_OTG_FS_DIEPEMPMSK.  RW   $" OTG_FS_DEVICE_OTG_FS_DIEPEMPMSK @ hex. OTG_FS_DEVICE_OTG_FS_DIEPEMPMSK 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DIEPCTL0. cr ." OTG_FS_DEVICE_OTG_FS_DIEPCTL0.   $" OTG_FS_DEVICE_OTG_FS_DIEPCTL0 @ hex. OTG_FS_DEVICE_OTG_FS_DIEPCTL0 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DIEPCTL1. cr ." OTG_FS_DEVICE_OTG_FS_DIEPCTL1.   $" OTG_FS_DEVICE_OTG_FS_DIEPCTL1 @ hex. OTG_FS_DEVICE_OTG_FS_DIEPCTL1 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DIEPCTL2. cr ." OTG_FS_DEVICE_OTG_FS_DIEPCTL2.   $" OTG_FS_DEVICE_OTG_FS_DIEPCTL2 @ hex. OTG_FS_DEVICE_OTG_FS_DIEPCTL2 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DIEPCTL3. cr ." OTG_FS_DEVICE_OTG_FS_DIEPCTL3.   $" OTG_FS_DEVICE_OTG_FS_DIEPCTL3 @ hex. OTG_FS_DEVICE_OTG_FS_DIEPCTL3 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DOEPCTL0. cr ." OTG_FS_DEVICE_OTG_FS_DOEPCTL0.   $" OTG_FS_DEVICE_OTG_FS_DOEPCTL0 @ hex. OTG_FS_DEVICE_OTG_FS_DOEPCTL0 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DOEPCTL1. cr ." OTG_FS_DEVICE_OTG_FS_DOEPCTL1.   $" OTG_FS_DEVICE_OTG_FS_DOEPCTL1 @ hex. OTG_FS_DEVICE_OTG_FS_DOEPCTL1 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DOEPCTL2. cr ." OTG_FS_DEVICE_OTG_FS_DOEPCTL2.   $" OTG_FS_DEVICE_OTG_FS_DOEPCTL2 @ hex. OTG_FS_DEVICE_OTG_FS_DOEPCTL2 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DOEPCTL3. cr ." OTG_FS_DEVICE_OTG_FS_DOEPCTL3.   $" OTG_FS_DEVICE_OTG_FS_DOEPCTL3 @ hex. OTG_FS_DEVICE_OTG_FS_DOEPCTL3 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DIEPINT0. cr ." OTG_FS_DEVICE_OTG_FS_DIEPINT0.   $" OTG_FS_DEVICE_OTG_FS_DIEPINT0 @ hex. OTG_FS_DEVICE_OTG_FS_DIEPINT0 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DIEPINT1. cr ." OTG_FS_DEVICE_OTG_FS_DIEPINT1.   $" OTG_FS_DEVICE_OTG_FS_DIEPINT1 @ hex. OTG_FS_DEVICE_OTG_FS_DIEPINT1 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DIEPINT2. cr ." OTG_FS_DEVICE_OTG_FS_DIEPINT2.   $" OTG_FS_DEVICE_OTG_FS_DIEPINT2 @ hex. OTG_FS_DEVICE_OTG_FS_DIEPINT2 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DIEPINT3. cr ." OTG_FS_DEVICE_OTG_FS_DIEPINT3.   $" OTG_FS_DEVICE_OTG_FS_DIEPINT3 @ hex. OTG_FS_DEVICE_OTG_FS_DIEPINT3 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DOEPINT0. cr ." OTG_FS_DEVICE_OTG_FS_DOEPINT0.  RW   $" OTG_FS_DEVICE_OTG_FS_DOEPINT0 @ hex. OTG_FS_DEVICE_OTG_FS_DOEPINT0 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DOEPINT1. cr ." OTG_FS_DEVICE_OTG_FS_DOEPINT1.  RW   $" OTG_FS_DEVICE_OTG_FS_DOEPINT1 @ hex. OTG_FS_DEVICE_OTG_FS_DOEPINT1 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DOEPINT2. cr ." OTG_FS_DEVICE_OTG_FS_DOEPINT2.  RW   $" OTG_FS_DEVICE_OTG_FS_DOEPINT2 @ hex. OTG_FS_DEVICE_OTG_FS_DOEPINT2 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DOEPINT3. cr ." OTG_FS_DEVICE_OTG_FS_DOEPINT3.  RW   $" OTG_FS_DEVICE_OTG_FS_DOEPINT3 @ hex. OTG_FS_DEVICE_OTG_FS_DOEPINT3 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DIEPTSIZ0. cr ." OTG_FS_DEVICE_OTG_FS_DIEPTSIZ0.  RW   $" OTG_FS_DEVICE_OTG_FS_DIEPTSIZ0 @ hex. OTG_FS_DEVICE_OTG_FS_DIEPTSIZ0 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DOEPTSIZ0. cr ." OTG_FS_DEVICE_OTG_FS_DOEPTSIZ0.  RW   $" OTG_FS_DEVICE_OTG_FS_DOEPTSIZ0 @ hex. OTG_FS_DEVICE_OTG_FS_DOEPTSIZ0 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DIEPTSIZ1. cr ." OTG_FS_DEVICE_OTG_FS_DIEPTSIZ1.  RW   $" OTG_FS_DEVICE_OTG_FS_DIEPTSIZ1 @ hex. OTG_FS_DEVICE_OTG_FS_DIEPTSIZ1 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DIEPTSIZ2. cr ." OTG_FS_DEVICE_OTG_FS_DIEPTSIZ2.  RW   $" OTG_FS_DEVICE_OTG_FS_DIEPTSIZ2 @ hex. OTG_FS_DEVICE_OTG_FS_DIEPTSIZ2 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DIEPTSIZ3. cr ." OTG_FS_DEVICE_OTG_FS_DIEPTSIZ3.  RW   $" OTG_FS_DEVICE_OTG_FS_DIEPTSIZ3 @ hex. OTG_FS_DEVICE_OTG_FS_DIEPTSIZ3 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DTXFSTS0. cr ." OTG_FS_DEVICE_OTG_FS_DTXFSTS0.  RO   $" OTG_FS_DEVICE_OTG_FS_DTXFSTS0 @ hex. OTG_FS_DEVICE_OTG_FS_DTXFSTS0 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DTXFSTS1. cr ." OTG_FS_DEVICE_OTG_FS_DTXFSTS1.  RO   $" OTG_FS_DEVICE_OTG_FS_DTXFSTS1 @ hex. OTG_FS_DEVICE_OTG_FS_DTXFSTS1 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DTXFSTS2. cr ." OTG_FS_DEVICE_OTG_FS_DTXFSTS2.  RO   $" OTG_FS_DEVICE_OTG_FS_DTXFSTS2 @ hex. OTG_FS_DEVICE_OTG_FS_DTXFSTS2 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DTXFSTS3. cr ." OTG_FS_DEVICE_OTG_FS_DTXFSTS3.  RO   $" OTG_FS_DEVICE_OTG_FS_DTXFSTS3 @ hex. OTG_FS_DEVICE_OTG_FS_DTXFSTS3 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DOEPTSIZ1. cr ." OTG_FS_DEVICE_OTG_FS_DOEPTSIZ1.  RW   $" OTG_FS_DEVICE_OTG_FS_DOEPTSIZ1 @ hex. OTG_FS_DEVICE_OTG_FS_DOEPTSIZ1 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DOEPTSIZ2. cr ." OTG_FS_DEVICE_OTG_FS_DOEPTSIZ2.  RW   $" OTG_FS_DEVICE_OTG_FS_DOEPTSIZ2 @ hex. OTG_FS_DEVICE_OTG_FS_DOEPTSIZ2 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DOEPTSIZ3. cr ." OTG_FS_DEVICE_OTG_FS_DOEPTSIZ3.  RW   $" OTG_FS_DEVICE_OTG_FS_DOEPTSIZ3 @ hex. OTG_FS_DEVICE_OTG_FS_DOEPTSIZ3 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DIEPCTL4. cr ." OTG_FS_DEVICE_OTG_FS_DIEPCTL4.   $" OTG_FS_DEVICE_OTG_FS_DIEPCTL4 @ hex. OTG_FS_DEVICE_OTG_FS_DIEPCTL4 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DIEPINT4. cr ." OTG_FS_DEVICE_OTG_FS_DIEPINT4.   $" OTG_FS_DEVICE_OTG_FS_DIEPINT4 @ hex. OTG_FS_DEVICE_OTG_FS_DIEPINT4 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DIEPTSIZ4. cr ." OTG_FS_DEVICE_OTG_FS_DIEPTSIZ4.  RW   $" OTG_FS_DEVICE_OTG_FS_DIEPTSIZ4 @ hex. OTG_FS_DEVICE_OTG_FS_DIEPTSIZ4 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DTXFSTS4. cr ." OTG_FS_DEVICE_OTG_FS_DTXFSTS4.  RW   $" OTG_FS_DEVICE_OTG_FS_DTXFSTS4 @ hex. OTG_FS_DEVICE_OTG_FS_DTXFSTS4 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DIEPCTL5. cr ." OTG_FS_DEVICE_OTG_FS_DIEPCTL5.   $" OTG_FS_DEVICE_OTG_FS_DIEPCTL5 @ hex. OTG_FS_DEVICE_OTG_FS_DIEPCTL5 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DIEPINT5. cr ." OTG_FS_DEVICE_OTG_FS_DIEPINT5.   $" OTG_FS_DEVICE_OTG_FS_DIEPINT5 @ hex. OTG_FS_DEVICE_OTG_FS_DIEPINT5 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DIEPTSIZ55. cr ." OTG_FS_DEVICE_OTG_FS_DIEPTSIZ55.  RW   $" OTG_FS_DEVICE_OTG_FS_DIEPTSIZ55 @ hex. OTG_FS_DEVICE_OTG_FS_DIEPTSIZ55 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DTXFSTS55. cr ." OTG_FS_DEVICE_OTG_FS_DTXFSTS55.  RW   $" OTG_FS_DEVICE_OTG_FS_DTXFSTS55 @ hex. OTG_FS_DEVICE_OTG_FS_DTXFSTS55 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DOEPCTL4. cr ." OTG_FS_DEVICE_OTG_FS_DOEPCTL4.   $" OTG_FS_DEVICE_OTG_FS_DOEPCTL4 @ hex. OTG_FS_DEVICE_OTG_FS_DOEPCTL4 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DOEPINT4. cr ." OTG_FS_DEVICE_OTG_FS_DOEPINT4.  RW   $" OTG_FS_DEVICE_OTG_FS_DOEPINT4 @ hex. OTG_FS_DEVICE_OTG_FS_DOEPINT4 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DOEPTSIZ4. cr ." OTG_FS_DEVICE_OTG_FS_DOEPTSIZ4.  RW   $" OTG_FS_DEVICE_OTG_FS_DOEPTSIZ4 @ hex. OTG_FS_DEVICE_OTG_FS_DOEPTSIZ4 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DOEPCTL5. cr ." OTG_FS_DEVICE_OTG_FS_DOEPCTL5.   $" OTG_FS_DEVICE_OTG_FS_DOEPCTL5 @ hex. OTG_FS_DEVICE_OTG_FS_DOEPCTL5 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DOEPINT5. cr ." OTG_FS_DEVICE_OTG_FS_DOEPINT5.  RW   $" OTG_FS_DEVICE_OTG_FS_DOEPINT5 @ hex. OTG_FS_DEVICE_OTG_FS_DOEPINT5 1b. ;
    : OTG_FS_DEVICE_OTG_FS_DOEPTSIZ5. cr ." OTG_FS_DEVICE_OTG_FS_DOEPTSIZ5.  RW   $" OTG_FS_DEVICE_OTG_FS_DOEPTSIZ5 @ hex. OTG_FS_DEVICE_OTG_FS_DOEPTSIZ5 1b. ;
    : OTG_FS_DEVICE.
      OTG_FS_DEVICE_OTG_FS_DCFG.
      OTG_FS_DEVICE_OTG_FS_DCTL.
      OTG_FS_DEVICE_OTG_FS_DSTS.
      OTG_FS_DEVICE_OTG_FS_DIEPMSK.
      OTG_FS_DEVICE_OTG_FS_DOEPMSK.
      OTG_FS_DEVICE_OTG_FS_DAINT.
      OTG_FS_DEVICE_OTG_FS_DAINTMSK.
      OTG_FS_DEVICE_OTG_FS_DVBUSDIS.
      OTG_FS_DEVICE_OTG_FS_DVBUSPULSE.
      OTG_FS_DEVICE_OTG_FS_DIEPEMPMSK.
      OTG_FS_DEVICE_OTG_FS_DIEPCTL0.
      OTG_FS_DEVICE_OTG_FS_DIEPCTL1.
      OTG_FS_DEVICE_OTG_FS_DIEPCTL2.
      OTG_FS_DEVICE_OTG_FS_DIEPCTL3.
      OTG_FS_DEVICE_OTG_FS_DOEPCTL0.
      OTG_FS_DEVICE_OTG_FS_DOEPCTL1.
      OTG_FS_DEVICE_OTG_FS_DOEPCTL2.
      OTG_FS_DEVICE_OTG_FS_DOEPCTL3.
      OTG_FS_DEVICE_OTG_FS_DIEPINT0.
      OTG_FS_DEVICE_OTG_FS_DIEPINT1.
      OTG_FS_DEVICE_OTG_FS_DIEPINT2.
      OTG_FS_DEVICE_OTG_FS_DIEPINT3.
      OTG_FS_DEVICE_OTG_FS_DOEPINT0.
      OTG_FS_DEVICE_OTG_FS_DOEPINT1.
      OTG_FS_DEVICE_OTG_FS_DOEPINT2.
      OTG_FS_DEVICE_OTG_FS_DOEPINT3.
      OTG_FS_DEVICE_OTG_FS_DIEPTSIZ0.
      OTG_FS_DEVICE_OTG_FS_DOEPTSIZ0.
      OTG_FS_DEVICE_OTG_FS_DIEPTSIZ1.
      OTG_FS_DEVICE_OTG_FS_DIEPTSIZ2.
      OTG_FS_DEVICE_OTG_FS_DIEPTSIZ3.
      OTG_FS_DEVICE_OTG_FS_DTXFSTS0.
      OTG_FS_DEVICE_OTG_FS_DTXFSTS1.
      OTG_FS_DEVICE_OTG_FS_DTXFSTS2.
      OTG_FS_DEVICE_OTG_FS_DTXFSTS3.
      OTG_FS_DEVICE_OTG_FS_DOEPTSIZ1.
      OTG_FS_DEVICE_OTG_FS_DOEPTSIZ2.
      OTG_FS_DEVICE_OTG_FS_DOEPTSIZ3.
      OTG_FS_DEVICE_OTG_FS_DIEPCTL4.
      OTG_FS_DEVICE_OTG_FS_DIEPINT4.
      OTG_FS_DEVICE_OTG_FS_DIEPTSIZ4.
      OTG_FS_DEVICE_OTG_FS_DTXFSTS4.
      OTG_FS_DEVICE_OTG_FS_DIEPCTL5.
      OTG_FS_DEVICE_OTG_FS_DIEPINT5.
      OTG_FS_DEVICE_OTG_FS_DIEPTSIZ55.
      OTG_FS_DEVICE_OTG_FS_DTXFSTS55.
      OTG_FS_DEVICE_OTG_FS_DOEPCTL4.
      OTG_FS_DEVICE_OTG_FS_DOEPINT4.
      OTG_FS_DEVICE_OTG_FS_DOEPTSIZ4.
      OTG_FS_DEVICE_OTG_FS_DOEPCTL5.
      OTG_FS_DEVICE_OTG_FS_DOEPINT5.
      OTG_FS_DEVICE_OTG_FS_DOEPTSIZ5.
    ;
  [then]

  execute-defined? use-OTG_FS_PWRCLK [if]
    $50000E00 constant OTG_FS_PWRCLK ( USB on the go full speed ) 
    OTG_FS_PWRCLK $0 + constant OTG_FS_PWRCLK_OTG_FS_PCGCCTL ( read-write )  \ OTG_FS power and clock gating control register OTG_FS_PCGCCTL
    : OTG_FS_PWRCLK_OTG_FS_PCGCCTL. cr ." OTG_FS_PWRCLK_OTG_FS_PCGCCTL.  RW   $" OTG_FS_PWRCLK_OTG_FS_PCGCCTL @ hex. OTG_FS_PWRCLK_OTG_FS_PCGCCTL 1b. ;
    : OTG_FS_PWRCLK.
      OTG_FS_PWRCLK_OTG_FS_PCGCCTL.
    ;
  [then]

  execute-defined? use-OTG_HS_GLOBAL [if]
    $40040000 constant OTG_HS_GLOBAL ( USB on the go high speed ) 
    OTG_HS_GLOBAL $0 + constant OTG_HS_GLOBAL_OTG_HS_GOTGCTL (  )  \ OTG_HS control and status register
    OTG_HS_GLOBAL $4 + constant OTG_HS_GLOBAL_OTG_HS_GOTGINT ( read-write )  \ OTG_HS interrupt register
    OTG_HS_GLOBAL $8 + constant OTG_HS_GLOBAL_OTG_HS_GAHBCFG ( read-write )  \ OTG_HS AHB configuration register
    OTG_HS_GLOBAL $C + constant OTG_HS_GLOBAL_OTG_HS_GUSBCFG (  )  \ OTG_HS USB configuration register
    OTG_HS_GLOBAL $10 + constant OTG_HS_GLOBAL_OTG_HS_GRSTCTL (  )  \ OTG_HS reset register
    OTG_HS_GLOBAL $14 + constant OTG_HS_GLOBAL_OTG_HS_GINTSTS (  )  \ OTG_HS core interrupt register
    OTG_HS_GLOBAL $18 + constant OTG_HS_GLOBAL_OTG_HS_GINTMSK (  )  \ OTG_HS interrupt mask register
    OTG_HS_GLOBAL $1C + constant OTG_HS_GLOBAL_OTG_HS_GRXSTSR_Host ( read-only )  \ OTG_HS Receive status debug read register host mode
    OTG_HS_GLOBAL $20 + constant OTG_HS_GLOBAL_OTG_HS_GRXSTSP_Host ( read-only )  \ OTG_HS status read and pop register host mode
    OTG_HS_GLOBAL $24 + constant OTG_HS_GLOBAL_OTG_HS_GRXFSIZ ( read-write )  \ OTG_HS Receive FIFO size register
    OTG_HS_GLOBAL $28 + constant OTG_HS_GLOBAL_OTG_HS_HNPTXFSIZ_Host ( read-write )  \ OTG_HS nonperiodic transmit FIFO size register host mode
    OTG_HS_GLOBAL $28 + constant OTG_HS_GLOBAL_OTG_HS_DIEPTXF0_Device ( read-write )  \ Endpoint 0 transmit FIFO size peripheral mode
    OTG_HS_GLOBAL $2C + constant OTG_HS_GLOBAL_OTG_HS_GNPTXSTS ( read-only )  \ OTG_HS nonperiodic transmit FIFO/queue status register
    OTG_HS_GLOBAL $38 + constant OTG_HS_GLOBAL_OTG_HS_GCCFG ( read-write )  \ OTG_HS general core configuration register
    OTG_HS_GLOBAL $3C + constant OTG_HS_GLOBAL_OTG_HS_CID ( read-write )  \ OTG_HS core ID register
    OTG_HS_GLOBAL $100 + constant OTG_HS_GLOBAL_OTG_HS_HPTXFSIZ ( read-write )  \ OTG_HS Host periodic transmit FIFO size register
    OTG_HS_GLOBAL $104 + constant OTG_HS_GLOBAL_OTG_HS_DIEPTXF1 ( read-write )  \ OTG_HS device IN endpoint transmit FIFO size register
    OTG_HS_GLOBAL $108 + constant OTG_HS_GLOBAL_OTG_HS_DIEPTXF2 ( read-write )  \ OTG_HS device IN endpoint transmit FIFO size register
    OTG_HS_GLOBAL $11C + constant OTG_HS_GLOBAL_OTG_HS_DIEPTXF3 ( read-write )  \ OTG_HS device IN endpoint transmit FIFO size register
    OTG_HS_GLOBAL $120 + constant OTG_HS_GLOBAL_OTG_HS_DIEPTXF4 ( read-write )  \ OTG_HS device IN endpoint transmit FIFO size register
    OTG_HS_GLOBAL $124 + constant OTG_HS_GLOBAL_OTG_HS_DIEPTXF5 ( read-write )  \ OTG_HS device IN endpoint transmit FIFO size register
    OTG_HS_GLOBAL $128 + constant OTG_HS_GLOBAL_OTG_HS_DIEPTXF6 ( read-write )  \ OTG_HS device IN endpoint transmit FIFO size register
    OTG_HS_GLOBAL $12C + constant OTG_HS_GLOBAL_OTG_HS_DIEPTXF7 ( read-write )  \ OTG_HS device IN endpoint transmit FIFO size register
    OTG_HS_GLOBAL $1C + constant OTG_HS_GLOBAL_OTG_HS_GRXSTSR_Device ( read-only )  \ OTG_HS Receive status debug read register peripheral mode mode
    OTG_HS_GLOBAL $20 + constant OTG_HS_GLOBAL_OTG_HS_GRXSTSP_Device ( read-only )  \ OTG_HS status read and pop register peripheral mode
    OTG_HS_GLOBAL $54 + constant OTG_HS_GLOBAL_OTG_HS_GLPMCFG (  )  \ OTG core LPM configuration register
    : OTG_HS_GLOBAL_OTG_HS_GOTGCTL. cr ." OTG_HS_GLOBAL_OTG_HS_GOTGCTL.   $" OTG_HS_GLOBAL_OTG_HS_GOTGCTL @ hex. OTG_HS_GLOBAL_OTG_HS_GOTGCTL 1b. ;
    : OTG_HS_GLOBAL_OTG_HS_GOTGINT. cr ." OTG_HS_GLOBAL_OTG_HS_GOTGINT.  RW   $" OTG_HS_GLOBAL_OTG_HS_GOTGINT @ hex. OTG_HS_GLOBAL_OTG_HS_GOTGINT 1b. ;
    : OTG_HS_GLOBAL_OTG_HS_GAHBCFG. cr ." OTG_HS_GLOBAL_OTG_HS_GAHBCFG.  RW   $" OTG_HS_GLOBAL_OTG_HS_GAHBCFG @ hex. OTG_HS_GLOBAL_OTG_HS_GAHBCFG 1b. ;
    : OTG_HS_GLOBAL_OTG_HS_GUSBCFG. cr ." OTG_HS_GLOBAL_OTG_HS_GUSBCFG.   $" OTG_HS_GLOBAL_OTG_HS_GUSBCFG @ hex. OTG_HS_GLOBAL_OTG_HS_GUSBCFG 1b. ;
    : OTG_HS_GLOBAL_OTG_HS_GRSTCTL. cr ." OTG_HS_GLOBAL_OTG_HS_GRSTCTL.   $" OTG_HS_GLOBAL_OTG_HS_GRSTCTL @ hex. OTG_HS_GLOBAL_OTG_HS_GRSTCTL 1b. ;
    : OTG_HS_GLOBAL_OTG_HS_GINTSTS. cr ." OTG_HS_GLOBAL_OTG_HS_GINTSTS.   $" OTG_HS_GLOBAL_OTG_HS_GINTSTS @ hex. OTG_HS_GLOBAL_OTG_HS_GINTSTS 1b. ;
    : OTG_HS_GLOBAL_OTG_HS_GINTMSK. cr ." OTG_HS_GLOBAL_OTG_HS_GINTMSK.   $" OTG_HS_GLOBAL_OTG_HS_GINTMSK @ hex. OTG_HS_GLOBAL_OTG_HS_GINTMSK 1b. ;
    : OTG_HS_GLOBAL_OTG_HS_GRXSTSR_Host. cr ." OTG_HS_GLOBAL_OTG_HS_GRXSTSR_Host.  RO   $" OTG_HS_GLOBAL_OTG_HS_GRXSTSR_Host @ hex. OTG_HS_GLOBAL_OTG_HS_GRXSTSR_Host 1b. ;
    : OTG_HS_GLOBAL_OTG_HS_GRXSTSP_Host. cr ." OTG_HS_GLOBAL_OTG_HS_GRXSTSP_Host.  RO   $" OTG_HS_GLOBAL_OTG_HS_GRXSTSP_Host @ hex. OTG_HS_GLOBAL_OTG_HS_GRXSTSP_Host 1b. ;
    : OTG_HS_GLOBAL_OTG_HS_GRXFSIZ. cr ." OTG_HS_GLOBAL_OTG_HS_GRXFSIZ.  RW   $" OTG_HS_GLOBAL_OTG_HS_GRXFSIZ @ hex. OTG_HS_GLOBAL_OTG_HS_GRXFSIZ 1b. ;
    : OTG_HS_GLOBAL_OTG_HS_HNPTXFSIZ_Host. cr ." OTG_HS_GLOBAL_OTG_HS_HNPTXFSIZ_Host.  RW   $" OTG_HS_GLOBAL_OTG_HS_HNPTXFSIZ_Host @ hex. OTG_HS_GLOBAL_OTG_HS_HNPTXFSIZ_Host 1b. ;
    : OTG_HS_GLOBAL_OTG_HS_DIEPTXF0_Device. cr ." OTG_HS_GLOBAL_OTG_HS_DIEPTXF0_Device.  RW   $" OTG_HS_GLOBAL_OTG_HS_DIEPTXF0_Device @ hex. OTG_HS_GLOBAL_OTG_HS_DIEPTXF0_Device 1b. ;
    : OTG_HS_GLOBAL_OTG_HS_GNPTXSTS. cr ." OTG_HS_GLOBAL_OTG_HS_GNPTXSTS.  RO   $" OTG_HS_GLOBAL_OTG_HS_GNPTXSTS @ hex. OTG_HS_GLOBAL_OTG_HS_GNPTXSTS 1b. ;
    : OTG_HS_GLOBAL_OTG_HS_GCCFG. cr ." OTG_HS_GLOBAL_OTG_HS_GCCFG.  RW   $" OTG_HS_GLOBAL_OTG_HS_GCCFG @ hex. OTG_HS_GLOBAL_OTG_HS_GCCFG 1b. ;
    : OTG_HS_GLOBAL_OTG_HS_CID. cr ." OTG_HS_GLOBAL_OTG_HS_CID.  RW   $" OTG_HS_GLOBAL_OTG_HS_CID @ hex. OTG_HS_GLOBAL_OTG_HS_CID 1b. ;
    : OTG_HS_GLOBAL_OTG_HS_HPTXFSIZ. cr ." OTG_HS_GLOBAL_OTG_HS_HPTXFSIZ.  RW   $" OTG_HS_GLOBAL_OTG_HS_HPTXFSIZ @ hex. OTG_HS_GLOBAL_OTG_HS_HPTXFSIZ 1b. ;
    : OTG_HS_GLOBAL_OTG_HS_DIEPTXF1. cr ." OTG_HS_GLOBAL_OTG_HS_DIEPTXF1.  RW   $" OTG_HS_GLOBAL_OTG_HS_DIEPTXF1 @ hex. OTG_HS_GLOBAL_OTG_HS_DIEPTXF1 1b. ;
    : OTG_HS_GLOBAL_OTG_HS_DIEPTXF2. cr ." OTG_HS_GLOBAL_OTG_HS_DIEPTXF2.  RW   $" OTG_HS_GLOBAL_OTG_HS_DIEPTXF2 @ hex. OTG_HS_GLOBAL_OTG_HS_DIEPTXF2 1b. ;
    : OTG_HS_GLOBAL_OTG_HS_DIEPTXF3. cr ." OTG_HS_GLOBAL_OTG_HS_DIEPTXF3.  RW   $" OTG_HS_GLOBAL_OTG_HS_DIEPTXF3 @ hex. OTG_HS_GLOBAL_OTG_HS_DIEPTXF3 1b. ;
    : OTG_HS_GLOBAL_OTG_HS_DIEPTXF4. cr ." OTG_HS_GLOBAL_OTG_HS_DIEPTXF4.  RW   $" OTG_HS_GLOBAL_OTG_HS_DIEPTXF4 @ hex. OTG_HS_GLOBAL_OTG_HS_DIEPTXF4 1b. ;
    : OTG_HS_GLOBAL_OTG_HS_DIEPTXF5. cr ." OTG_HS_GLOBAL_OTG_HS_DIEPTXF5.  RW   $" OTG_HS_GLOBAL_OTG_HS_DIEPTXF5 @ hex. OTG_HS_GLOBAL_OTG_HS_DIEPTXF5 1b. ;
    : OTG_HS_GLOBAL_OTG_HS_DIEPTXF6. cr ." OTG_HS_GLOBAL_OTG_HS_DIEPTXF6.  RW   $" OTG_HS_GLOBAL_OTG_HS_DIEPTXF6 @ hex. OTG_HS_GLOBAL_OTG_HS_DIEPTXF6 1b. ;
    : OTG_HS_GLOBAL_OTG_HS_DIEPTXF7. cr ." OTG_HS_GLOBAL_OTG_HS_DIEPTXF7.  RW   $" OTG_HS_GLOBAL_OTG_HS_DIEPTXF7 @ hex. OTG_HS_GLOBAL_OTG_HS_DIEPTXF7 1b. ;
    : OTG_HS_GLOBAL_OTG_HS_GRXSTSR_Device. cr ." OTG_HS_GLOBAL_OTG_HS_GRXSTSR_Device.  RO   $" OTG_HS_GLOBAL_OTG_HS_GRXSTSR_Device @ hex. OTG_HS_GLOBAL_OTG_HS_GRXSTSR_Device 1b. ;
    : OTG_HS_GLOBAL_OTG_HS_GRXSTSP_Device. cr ." OTG_HS_GLOBAL_OTG_HS_GRXSTSP_Device.  RO   $" OTG_HS_GLOBAL_OTG_HS_GRXSTSP_Device @ hex. OTG_HS_GLOBAL_OTG_HS_GRXSTSP_Device 1b. ;
    : OTG_HS_GLOBAL_OTG_HS_GLPMCFG. cr ." OTG_HS_GLOBAL_OTG_HS_GLPMCFG.   $" OTG_HS_GLOBAL_OTG_HS_GLPMCFG @ hex. OTG_HS_GLOBAL_OTG_HS_GLPMCFG 1b. ;
    : OTG_HS_GLOBAL.
      OTG_HS_GLOBAL_OTG_HS_GOTGCTL.
      OTG_HS_GLOBAL_OTG_HS_GOTGINT.
      OTG_HS_GLOBAL_OTG_HS_GAHBCFG.
      OTG_HS_GLOBAL_OTG_HS_GUSBCFG.
      OTG_HS_GLOBAL_OTG_HS_GRSTCTL.
      OTG_HS_GLOBAL_OTG_HS_GINTSTS.
      OTG_HS_GLOBAL_OTG_HS_GINTMSK.
      OTG_HS_GLOBAL_OTG_HS_GRXSTSR_Host.
      OTG_HS_GLOBAL_OTG_HS_GRXSTSP_Host.
      OTG_HS_GLOBAL_OTG_HS_GRXFSIZ.
      OTG_HS_GLOBAL_OTG_HS_HNPTXFSIZ_Host.
      OTG_HS_GLOBAL_OTG_HS_DIEPTXF0_Device.
      OTG_HS_GLOBAL_OTG_HS_GNPTXSTS.
      OTG_HS_GLOBAL_OTG_HS_GCCFG.
      OTG_HS_GLOBAL_OTG_HS_CID.
      OTG_HS_GLOBAL_OTG_HS_HPTXFSIZ.
      OTG_HS_GLOBAL_OTG_HS_DIEPTXF1.
      OTG_HS_GLOBAL_OTG_HS_DIEPTXF2.
      OTG_HS_GLOBAL_OTG_HS_DIEPTXF3.
      OTG_HS_GLOBAL_OTG_HS_DIEPTXF4.
      OTG_HS_GLOBAL_OTG_HS_DIEPTXF5.
      OTG_HS_GLOBAL_OTG_HS_DIEPTXF6.
      OTG_HS_GLOBAL_OTG_HS_DIEPTXF7.
      OTG_HS_GLOBAL_OTG_HS_GRXSTSR_Device.
      OTG_HS_GLOBAL_OTG_HS_GRXSTSP_Device.
      OTG_HS_GLOBAL_OTG_HS_GLPMCFG.
    ;
  [then]

  execute-defined? use-OTG_HS_HOST [if]
    $40040400 constant OTG_HS_HOST ( USB on the go high speed ) 
    OTG_HS_HOST $0 + constant OTG_HS_HOST_OTG_HS_HCFG (  )  \ OTG_HS host configuration register
    OTG_HS_HOST $4 + constant OTG_HS_HOST_OTG_HS_HFIR ( read-write )  \ OTG_HS Host frame interval register
    OTG_HS_HOST $8 + constant OTG_HS_HOST_OTG_HS_HFNUM ( read-only )  \ OTG_HS host frame number/frame time remaining register
    OTG_HS_HOST $10 + constant OTG_HS_HOST_OTG_HS_HPTXSTS (  )  \ OTG_HS_Host periodic transmit FIFO/queue status register
    OTG_HS_HOST $14 + constant OTG_HS_HOST_OTG_HS_HAINT ( read-only )  \ OTG_HS Host all channels interrupt register
    OTG_HS_HOST $18 + constant OTG_HS_HOST_OTG_HS_HAINTMSK ( read-write )  \ OTG_HS host all channels interrupt mask register
    OTG_HS_HOST $40 + constant OTG_HS_HOST_OTG_HS_HPRT (  )  \ OTG_HS host port control and status register
    OTG_HS_HOST $100 + constant OTG_HS_HOST_OTG_HS_HCCHAR0 ( read-write )  \ OTG_HS host channel-0 characteristics register
    OTG_HS_HOST $120 + constant OTG_HS_HOST_OTG_HS_HCCHAR1 ( read-write )  \ OTG_HS host channel-1 characteristics register
    OTG_HS_HOST $140 + constant OTG_HS_HOST_OTG_HS_HCCHAR2 ( read-write )  \ OTG_HS host channel-2 characteristics register
    OTG_HS_HOST $160 + constant OTG_HS_HOST_OTG_HS_HCCHAR3 ( read-write )  \ OTG_HS host channel-3 characteristics register
    OTG_HS_HOST $180 + constant OTG_HS_HOST_OTG_HS_HCCHAR4 ( read-write )  \ OTG_HS host channel-4 characteristics register
    OTG_HS_HOST $1A0 + constant OTG_HS_HOST_OTG_HS_HCCHAR5 ( read-write )  \ OTG_HS host channel-5 characteristics register
    OTG_HS_HOST $1C0 + constant OTG_HS_HOST_OTG_HS_HCCHAR6 ( read-write )  \ OTG_HS host channel-6 characteristics register
    OTG_HS_HOST $1E0 + constant OTG_HS_HOST_OTG_HS_HCCHAR7 ( read-write )  \ OTG_HS host channel-7 characteristics register
    OTG_HS_HOST $200 + constant OTG_HS_HOST_OTG_HS_HCCHAR8 ( read-write )  \ OTG_HS host channel-8 characteristics register
    OTG_HS_HOST $220 + constant OTG_HS_HOST_OTG_HS_HCCHAR9 ( read-write )  \ OTG_HS host channel-9 characteristics register
    OTG_HS_HOST $240 + constant OTG_HS_HOST_OTG_HS_HCCHAR10 ( read-write )  \ OTG_HS host channel-10 characteristics register
    OTG_HS_HOST $260 + constant OTG_HS_HOST_OTG_HS_HCCHAR11 ( read-write )  \ OTG_HS host channel-11 characteristics register
    OTG_HS_HOST $104 + constant OTG_HS_HOST_OTG_HS_HCSPLT0 ( read-write )  \ OTG_HS host channel-0 split control register
    OTG_HS_HOST $124 + constant OTG_HS_HOST_OTG_HS_HCSPLT1 ( read-write )  \ OTG_HS host channel-1 split control register
    OTG_HS_HOST $144 + constant OTG_HS_HOST_OTG_HS_HCSPLT2 ( read-write )  \ OTG_HS host channel-2 split control register
    OTG_HS_HOST $164 + constant OTG_HS_HOST_OTG_HS_HCSPLT3 ( read-write )  \ OTG_HS host channel-3 split control register
    OTG_HS_HOST $184 + constant OTG_HS_HOST_OTG_HS_HCSPLT4 ( read-write )  \ OTG_HS host channel-4 split control register
    OTG_HS_HOST $1A4 + constant OTG_HS_HOST_OTG_HS_HCSPLT5 ( read-write )  \ OTG_HS host channel-5 split control register
    OTG_HS_HOST $1C4 + constant OTG_HS_HOST_OTG_HS_HCSPLT6 ( read-write )  \ OTG_HS host channel-6 split control register
    OTG_HS_HOST $1E4 + constant OTG_HS_HOST_OTG_HS_HCSPLT7 ( read-write )  \ OTG_HS host channel-7 split control register
    OTG_HS_HOST $204 + constant OTG_HS_HOST_OTG_HS_HCSPLT8 ( read-write )  \ OTG_HS host channel-8 split control register
    OTG_HS_HOST $224 + constant OTG_HS_HOST_OTG_HS_HCSPLT9 ( read-write )  \ OTG_HS host channel-9 split control register
    OTG_HS_HOST $244 + constant OTG_HS_HOST_OTG_HS_HCSPLT10 ( read-write )  \ OTG_HS host channel-10 split control register
    OTG_HS_HOST $264 + constant OTG_HS_HOST_OTG_HS_HCSPLT11 ( read-write )  \ OTG_HS host channel-11 split control register
    OTG_HS_HOST $108 + constant OTG_HS_HOST_OTG_HS_HCINT0 ( read-write )  \ OTG_HS host channel-11 interrupt register
    OTG_HS_HOST $128 + constant OTG_HS_HOST_OTG_HS_HCINT1 ( read-write )  \ OTG_HS host channel-1 interrupt register
    OTG_HS_HOST $148 + constant OTG_HS_HOST_OTG_HS_HCINT2 ( read-write )  \ OTG_HS host channel-2 interrupt register
    OTG_HS_HOST $168 + constant OTG_HS_HOST_OTG_HS_HCINT3 ( read-write )  \ OTG_HS host channel-3 interrupt register
    OTG_HS_HOST $188 + constant OTG_HS_HOST_OTG_HS_HCINT4 ( read-write )  \ OTG_HS host channel-4 interrupt register
    OTG_HS_HOST $1A8 + constant OTG_HS_HOST_OTG_HS_HCINT5 ( read-write )  \ OTG_HS host channel-5 interrupt register
    OTG_HS_HOST $1C8 + constant OTG_HS_HOST_OTG_HS_HCINT6 ( read-write )  \ OTG_HS host channel-6 interrupt register
    OTG_HS_HOST $1E8 + constant OTG_HS_HOST_OTG_HS_HCINT7 ( read-write )  \ OTG_HS host channel-7 interrupt register
    OTG_HS_HOST $208 + constant OTG_HS_HOST_OTG_HS_HCINT8 ( read-write )  \ OTG_HS host channel-8 interrupt register
    OTG_HS_HOST $228 + constant OTG_HS_HOST_OTG_HS_HCINT9 ( read-write )  \ OTG_HS host channel-9 interrupt register
    OTG_HS_HOST $248 + constant OTG_HS_HOST_OTG_HS_HCINT10 ( read-write )  \ OTG_HS host channel-10 interrupt register
    OTG_HS_HOST $268 + constant OTG_HS_HOST_OTG_HS_HCINT11 ( read-write )  \ OTG_HS host channel-11 interrupt register
    OTG_HS_HOST $10C + constant OTG_HS_HOST_OTG_HS_HCINTMSK0 ( read-write )  \ OTG_HS host channel-11 interrupt mask register
    OTG_HS_HOST $12C + constant OTG_HS_HOST_OTG_HS_HCINTMSK1 ( read-write )  \ OTG_HS host channel-1 interrupt mask register
    OTG_HS_HOST $14C + constant OTG_HS_HOST_OTG_HS_HCINTMSK2 ( read-write )  \ OTG_HS host channel-2 interrupt mask register
    OTG_HS_HOST $16C + constant OTG_HS_HOST_OTG_HS_HCINTMSK3 ( read-write )  \ OTG_HS host channel-3 interrupt mask register
    OTG_HS_HOST $18C + constant OTG_HS_HOST_OTG_HS_HCINTMSK4 ( read-write )  \ OTG_HS host channel-4 interrupt mask register
    OTG_HS_HOST $1AC + constant OTG_HS_HOST_OTG_HS_HCINTMSK5 ( read-write )  \ OTG_HS host channel-5 interrupt mask register
    OTG_HS_HOST $1CC + constant OTG_HS_HOST_OTG_HS_HCINTMSK6 ( read-write )  \ OTG_HS host channel-6 interrupt mask register
    OTG_HS_HOST $1EC + constant OTG_HS_HOST_OTG_HS_HCINTMSK7 ( read-write )  \ OTG_HS host channel-7 interrupt mask register
    OTG_HS_HOST $20C + constant OTG_HS_HOST_OTG_HS_HCINTMSK8 ( read-write )  \ OTG_HS host channel-8 interrupt mask register
    OTG_HS_HOST $22C + constant OTG_HS_HOST_OTG_HS_HCINTMSK9 ( read-write )  \ OTG_HS host channel-9 interrupt mask register
    OTG_HS_HOST $24C + constant OTG_HS_HOST_OTG_HS_HCINTMSK10 ( read-write )  \ OTG_HS host channel-10 interrupt mask register
    OTG_HS_HOST $26C + constant OTG_HS_HOST_OTG_HS_HCINTMSK11 ( read-write )  \ OTG_HS host channel-11 interrupt mask register
    OTG_HS_HOST $110 + constant OTG_HS_HOST_OTG_HS_HCTSIZ0 ( read-write )  \ OTG_HS host channel-11 transfer size register
    OTG_HS_HOST $130 + constant OTG_HS_HOST_OTG_HS_HCTSIZ1 ( read-write )  \ OTG_HS host channel-1 transfer size register
    OTG_HS_HOST $150 + constant OTG_HS_HOST_OTG_HS_HCTSIZ2 ( read-write )  \ OTG_HS host channel-2 transfer size register
    OTG_HS_HOST $170 + constant OTG_HS_HOST_OTG_HS_HCTSIZ3 ( read-write )  \ OTG_HS host channel-3 transfer size register
    OTG_HS_HOST $190 + constant OTG_HS_HOST_OTG_HS_HCTSIZ4 ( read-write )  \ OTG_HS host channel-4 transfer size register
    OTG_HS_HOST $1B0 + constant OTG_HS_HOST_OTG_HS_HCTSIZ5 ( read-write )  \ OTG_HS host channel-5 transfer size register
    OTG_HS_HOST $1D0 + constant OTG_HS_HOST_OTG_HS_HCTSIZ6 ( read-write )  \ OTG_HS host channel-6 transfer size register
    OTG_HS_HOST $1F0 + constant OTG_HS_HOST_OTG_HS_HCTSIZ7 ( read-write )  \ OTG_HS host channel-7 transfer size register
    OTG_HS_HOST $210 + constant OTG_HS_HOST_OTG_HS_HCTSIZ8 ( read-write )  \ OTG_HS host channel-8 transfer size register
    OTG_HS_HOST $230 + constant OTG_HS_HOST_OTG_HS_HCTSIZ9 ( read-write )  \ OTG_HS host channel-9 transfer size register
    OTG_HS_HOST $250 + constant OTG_HS_HOST_OTG_HS_HCTSIZ10 ( read-write )  \ OTG_HS host channel-10 transfer size register
    OTG_HS_HOST $270 + constant OTG_HS_HOST_OTG_HS_HCTSIZ11 ( read-write )  \ OTG_HS host channel-11 transfer size register
    OTG_HS_HOST $114 + constant OTG_HS_HOST_OTG_HS_HCDMA0 ( read-write )  \ OTG_HS host channel-0 DMA address register
    OTG_HS_HOST $134 + constant OTG_HS_HOST_OTG_HS_HCDMA1 ( read-write )  \ OTG_HS host channel-1 DMA address register
    OTG_HS_HOST $154 + constant OTG_HS_HOST_OTG_HS_HCDMA2 ( read-write )  \ OTG_HS host channel-2 DMA address register
    OTG_HS_HOST $174 + constant OTG_HS_HOST_OTG_HS_HCDMA3 ( read-write )  \ OTG_HS host channel-3 DMA address register
    OTG_HS_HOST $194 + constant OTG_HS_HOST_OTG_HS_HCDMA4 ( read-write )  \ OTG_HS host channel-4 DMA address register
    OTG_HS_HOST $1B4 + constant OTG_HS_HOST_OTG_HS_HCDMA5 ( read-write )  \ OTG_HS host channel-5 DMA address register
    OTG_HS_HOST $1D4 + constant OTG_HS_HOST_OTG_HS_HCDMA6 ( read-write )  \ OTG_HS host channel-6 DMA address register
    OTG_HS_HOST $1F4 + constant OTG_HS_HOST_OTG_HS_HCDMA7 ( read-write )  \ OTG_HS host channel-7 DMA address register
    OTG_HS_HOST $214 + constant OTG_HS_HOST_OTG_HS_HCDMA8 ( read-write )  \ OTG_HS host channel-8 DMA address register
    OTG_HS_HOST $234 + constant OTG_HS_HOST_OTG_HS_HCDMA9 ( read-write )  \ OTG_HS host channel-9 DMA address register
    OTG_HS_HOST $254 + constant OTG_HS_HOST_OTG_HS_HCDMA10 ( read-write )  \ OTG_HS host channel-10 DMA address register
    OTG_HS_HOST $274 + constant OTG_HS_HOST_OTG_HS_HCDMA11 ( read-write )  \ OTG_HS host channel-11 DMA address register
    OTG_HS_HOST $278 + constant OTG_HS_HOST_OTG_HS_HCCHAR12 ( read-write )  \ OTG_HS host channel-12 characteristics register
    OTG_HS_HOST $27C + constant OTG_HS_HOST_OTG_HS_HCSPLT12 ( read-write )  \ OTG_HS host channel-12 split control register
    OTG_HS_HOST $280 + constant OTG_HS_HOST_OTG_HS_HCINT12 ( read-write )  \ OTG_HS host channel-12 interrupt register
    OTG_HS_HOST $284 + constant OTG_HS_HOST_OTG_HS_HCINTMSK12 ( read-write )  \ OTG_HS host channel-12 interrupt mask register
    OTG_HS_HOST $288 + constant OTG_HS_HOST_OTG_HS_HCTSIZ12 ( read-write )  \ OTG_HS host channel-12 transfer size register
    OTG_HS_HOST $28C + constant OTG_HS_HOST_OTG_HS_HCDMA12 ( read-write )  \ OTG_HS host channel-12 DMA address register
    OTG_HS_HOST $290 + constant OTG_HS_HOST_OTG_HS_HCCHAR13 ( read-write )  \ OTG_HS host channel-13 characteristics register
    OTG_HS_HOST $294 + constant OTG_HS_HOST_OTG_HS_HCSPLT13 ( read-write )  \ OTG_HS host channel-13 split control register
    OTG_HS_HOST $298 + constant OTG_HS_HOST_OTG_HS_HCINT13 ( read-write )  \ OTG_HS host channel-13 interrupt register
    OTG_HS_HOST $29C + constant OTG_HS_HOST_OTG_HS_HCINTMSK13 ( read-write )  \ OTG_HS host channel-13 interrupt mask register
    OTG_HS_HOST $2A0 + constant OTG_HS_HOST_OTG_HS_HCTSIZ13 ( read-write )  \ OTG_HS host channel-13 transfer size register
    OTG_HS_HOST $2A4 + constant OTG_HS_HOST_OTG_HS_HCDMA13 ( read-write )  \ OTG_HS host channel-13 DMA address register
    OTG_HS_HOST $2A8 + constant OTG_HS_HOST_OTG_HS_HCCHAR14 ( read-write )  \ OTG_HS host channel-14 characteristics register
    OTG_HS_HOST $2AC + constant OTG_HS_HOST_OTG_HS_HCSPLT14 ( read-write )  \ OTG_HS host channel-14 split control register
    OTG_HS_HOST $2B0 + constant OTG_HS_HOST_OTG_HS_HCINT14 ( read-write )  \ OTG_HS host channel-14 interrupt register
    OTG_HS_HOST $2B4 + constant OTG_HS_HOST_OTG_HS_HCINTMSK14 ( read-write )  \ OTG_HS host channel-14 interrupt mask register
    OTG_HS_HOST $2B8 + constant OTG_HS_HOST_OTG_HS_HCTSIZ14 ( read-write )  \ OTG_HS host channel-14 transfer size register
    OTG_HS_HOST $2BC + constant OTG_HS_HOST_OTG_HS_HCDMA14 ( read-write )  \ OTG_HS host channel-14 DMA address register
    OTG_HS_HOST $2C0 + constant OTG_HS_HOST_OTG_HS_HCCHAR15 ( read-write )  \ OTG_HS host channel-15 characteristics register
    OTG_HS_HOST $2C4 + constant OTG_HS_HOST_OTG_HS_HCSPLT15 ( read-write )  \ OTG_HS host channel-15 split control register
    OTG_HS_HOST $2C8 + constant OTG_HS_HOST_OTG_HS_HCINT15 ( read-write )  \ OTG_HS host channel-15 interrupt register
    OTG_HS_HOST $2CC + constant OTG_HS_HOST_OTG_HS_HCINTMSK15 ( read-write )  \ OTG_HS host channel-15 interrupt mask register
    OTG_HS_HOST $2D0 + constant OTG_HS_HOST_OTG_HS_HCTSIZ15 ( read-write )  \ OTG_HS host channel-15 transfer size register
    OTG_HS_HOST $2D4 + constant OTG_HS_HOST_OTG_HS_HCDMA15 ( read-write )  \ OTG_HS host channel-15 DMA address register
    : OTG_HS_HOST_OTG_HS_HCFG. cr ." OTG_HS_HOST_OTG_HS_HCFG.   $" OTG_HS_HOST_OTG_HS_HCFG @ hex. OTG_HS_HOST_OTG_HS_HCFG 1b. ;
    : OTG_HS_HOST_OTG_HS_HFIR. cr ." OTG_HS_HOST_OTG_HS_HFIR.  RW   $" OTG_HS_HOST_OTG_HS_HFIR @ hex. OTG_HS_HOST_OTG_HS_HFIR 1b. ;
    : OTG_HS_HOST_OTG_HS_HFNUM. cr ." OTG_HS_HOST_OTG_HS_HFNUM.  RO   $" OTG_HS_HOST_OTG_HS_HFNUM @ hex. OTG_HS_HOST_OTG_HS_HFNUM 1b. ;
    : OTG_HS_HOST_OTG_HS_HPTXSTS. cr ." OTG_HS_HOST_OTG_HS_HPTXSTS.   $" OTG_HS_HOST_OTG_HS_HPTXSTS @ hex. OTG_HS_HOST_OTG_HS_HPTXSTS 1b. ;
    : OTG_HS_HOST_OTG_HS_HAINT. cr ." OTG_HS_HOST_OTG_HS_HAINT.  RO   $" OTG_HS_HOST_OTG_HS_HAINT @ hex. OTG_HS_HOST_OTG_HS_HAINT 1b. ;
    : OTG_HS_HOST_OTG_HS_HAINTMSK. cr ." OTG_HS_HOST_OTG_HS_HAINTMSK.  RW   $" OTG_HS_HOST_OTG_HS_HAINTMSK @ hex. OTG_HS_HOST_OTG_HS_HAINTMSK 1b. ;
    : OTG_HS_HOST_OTG_HS_HPRT. cr ." OTG_HS_HOST_OTG_HS_HPRT.   $" OTG_HS_HOST_OTG_HS_HPRT @ hex. OTG_HS_HOST_OTG_HS_HPRT 1b. ;
    : OTG_HS_HOST_OTG_HS_HCCHAR0. cr ." OTG_HS_HOST_OTG_HS_HCCHAR0.  RW   $" OTG_HS_HOST_OTG_HS_HCCHAR0 @ hex. OTG_HS_HOST_OTG_HS_HCCHAR0 1b. ;
    : OTG_HS_HOST_OTG_HS_HCCHAR1. cr ." OTG_HS_HOST_OTG_HS_HCCHAR1.  RW   $" OTG_HS_HOST_OTG_HS_HCCHAR1 @ hex. OTG_HS_HOST_OTG_HS_HCCHAR1 1b. ;
    : OTG_HS_HOST_OTG_HS_HCCHAR2. cr ." OTG_HS_HOST_OTG_HS_HCCHAR2.  RW   $" OTG_HS_HOST_OTG_HS_HCCHAR2 @ hex. OTG_HS_HOST_OTG_HS_HCCHAR2 1b. ;
    : OTG_HS_HOST_OTG_HS_HCCHAR3. cr ." OTG_HS_HOST_OTG_HS_HCCHAR3.  RW   $" OTG_HS_HOST_OTG_HS_HCCHAR3 @ hex. OTG_HS_HOST_OTG_HS_HCCHAR3 1b. ;
    : OTG_HS_HOST_OTG_HS_HCCHAR4. cr ." OTG_HS_HOST_OTG_HS_HCCHAR4.  RW   $" OTG_HS_HOST_OTG_HS_HCCHAR4 @ hex. OTG_HS_HOST_OTG_HS_HCCHAR4 1b. ;
    : OTG_HS_HOST_OTG_HS_HCCHAR5. cr ." OTG_HS_HOST_OTG_HS_HCCHAR5.  RW   $" OTG_HS_HOST_OTG_HS_HCCHAR5 @ hex. OTG_HS_HOST_OTG_HS_HCCHAR5 1b. ;
    : OTG_HS_HOST_OTG_HS_HCCHAR6. cr ." OTG_HS_HOST_OTG_HS_HCCHAR6.  RW   $" OTG_HS_HOST_OTG_HS_HCCHAR6 @ hex. OTG_HS_HOST_OTG_HS_HCCHAR6 1b. ;
    : OTG_HS_HOST_OTG_HS_HCCHAR7. cr ." OTG_HS_HOST_OTG_HS_HCCHAR7.  RW   $" OTG_HS_HOST_OTG_HS_HCCHAR7 @ hex. OTG_HS_HOST_OTG_HS_HCCHAR7 1b. ;
    : OTG_HS_HOST_OTG_HS_HCCHAR8. cr ." OTG_HS_HOST_OTG_HS_HCCHAR8.  RW   $" OTG_HS_HOST_OTG_HS_HCCHAR8 @ hex. OTG_HS_HOST_OTG_HS_HCCHAR8 1b. ;
    : OTG_HS_HOST_OTG_HS_HCCHAR9. cr ." OTG_HS_HOST_OTG_HS_HCCHAR9.  RW   $" OTG_HS_HOST_OTG_HS_HCCHAR9 @ hex. OTG_HS_HOST_OTG_HS_HCCHAR9 1b. ;
    : OTG_HS_HOST_OTG_HS_HCCHAR10. cr ." OTG_HS_HOST_OTG_HS_HCCHAR10.  RW   $" OTG_HS_HOST_OTG_HS_HCCHAR10 @ hex. OTG_HS_HOST_OTG_HS_HCCHAR10 1b. ;
    : OTG_HS_HOST_OTG_HS_HCCHAR11. cr ." OTG_HS_HOST_OTG_HS_HCCHAR11.  RW   $" OTG_HS_HOST_OTG_HS_HCCHAR11 @ hex. OTG_HS_HOST_OTG_HS_HCCHAR11 1b. ;
    : OTG_HS_HOST_OTG_HS_HCSPLT0. cr ." OTG_HS_HOST_OTG_HS_HCSPLT0.  RW   $" OTG_HS_HOST_OTG_HS_HCSPLT0 @ hex. OTG_HS_HOST_OTG_HS_HCSPLT0 1b. ;
    : OTG_HS_HOST_OTG_HS_HCSPLT1. cr ." OTG_HS_HOST_OTG_HS_HCSPLT1.  RW   $" OTG_HS_HOST_OTG_HS_HCSPLT1 @ hex. OTG_HS_HOST_OTG_HS_HCSPLT1 1b. ;
    : OTG_HS_HOST_OTG_HS_HCSPLT2. cr ." OTG_HS_HOST_OTG_HS_HCSPLT2.  RW   $" OTG_HS_HOST_OTG_HS_HCSPLT2 @ hex. OTG_HS_HOST_OTG_HS_HCSPLT2 1b. ;
    : OTG_HS_HOST_OTG_HS_HCSPLT3. cr ." OTG_HS_HOST_OTG_HS_HCSPLT3.  RW   $" OTG_HS_HOST_OTG_HS_HCSPLT3 @ hex. OTG_HS_HOST_OTG_HS_HCSPLT3 1b. ;
    : OTG_HS_HOST_OTG_HS_HCSPLT4. cr ." OTG_HS_HOST_OTG_HS_HCSPLT4.  RW   $" OTG_HS_HOST_OTG_HS_HCSPLT4 @ hex. OTG_HS_HOST_OTG_HS_HCSPLT4 1b. ;
    : OTG_HS_HOST_OTG_HS_HCSPLT5. cr ." OTG_HS_HOST_OTG_HS_HCSPLT5.  RW   $" OTG_HS_HOST_OTG_HS_HCSPLT5 @ hex. OTG_HS_HOST_OTG_HS_HCSPLT5 1b. ;
    : OTG_HS_HOST_OTG_HS_HCSPLT6. cr ." OTG_HS_HOST_OTG_HS_HCSPLT6.  RW   $" OTG_HS_HOST_OTG_HS_HCSPLT6 @ hex. OTG_HS_HOST_OTG_HS_HCSPLT6 1b. ;
    : OTG_HS_HOST_OTG_HS_HCSPLT7. cr ." OTG_HS_HOST_OTG_HS_HCSPLT7.  RW   $" OTG_HS_HOST_OTG_HS_HCSPLT7 @ hex. OTG_HS_HOST_OTG_HS_HCSPLT7 1b. ;
    : OTG_HS_HOST_OTG_HS_HCSPLT8. cr ." OTG_HS_HOST_OTG_HS_HCSPLT8.  RW   $" OTG_HS_HOST_OTG_HS_HCSPLT8 @ hex. OTG_HS_HOST_OTG_HS_HCSPLT8 1b. ;
    : OTG_HS_HOST_OTG_HS_HCSPLT9. cr ." OTG_HS_HOST_OTG_HS_HCSPLT9.  RW   $" OTG_HS_HOST_OTG_HS_HCSPLT9 @ hex. OTG_HS_HOST_OTG_HS_HCSPLT9 1b. ;
    : OTG_HS_HOST_OTG_HS_HCSPLT10. cr ." OTG_HS_HOST_OTG_HS_HCSPLT10.  RW   $" OTG_HS_HOST_OTG_HS_HCSPLT10 @ hex. OTG_HS_HOST_OTG_HS_HCSPLT10 1b. ;
    : OTG_HS_HOST_OTG_HS_HCSPLT11. cr ." OTG_HS_HOST_OTG_HS_HCSPLT11.  RW   $" OTG_HS_HOST_OTG_HS_HCSPLT11 @ hex. OTG_HS_HOST_OTG_HS_HCSPLT11 1b. ;
    : OTG_HS_HOST_OTG_HS_HCINT0. cr ." OTG_HS_HOST_OTG_HS_HCINT0.  RW   $" OTG_HS_HOST_OTG_HS_HCINT0 @ hex. OTG_HS_HOST_OTG_HS_HCINT0 1b. ;
    : OTG_HS_HOST_OTG_HS_HCINT1. cr ." OTG_HS_HOST_OTG_HS_HCINT1.  RW   $" OTG_HS_HOST_OTG_HS_HCINT1 @ hex. OTG_HS_HOST_OTG_HS_HCINT1 1b. ;
    : OTG_HS_HOST_OTG_HS_HCINT2. cr ." OTG_HS_HOST_OTG_HS_HCINT2.  RW   $" OTG_HS_HOST_OTG_HS_HCINT2 @ hex. OTG_HS_HOST_OTG_HS_HCINT2 1b. ;
    : OTG_HS_HOST_OTG_HS_HCINT3. cr ." OTG_HS_HOST_OTG_HS_HCINT3.  RW   $" OTG_HS_HOST_OTG_HS_HCINT3 @ hex. OTG_HS_HOST_OTG_HS_HCINT3 1b. ;
    : OTG_HS_HOST_OTG_HS_HCINT4. cr ." OTG_HS_HOST_OTG_HS_HCINT4.  RW   $" OTG_HS_HOST_OTG_HS_HCINT4 @ hex. OTG_HS_HOST_OTG_HS_HCINT4 1b. ;
    : OTG_HS_HOST_OTG_HS_HCINT5. cr ." OTG_HS_HOST_OTG_HS_HCINT5.  RW   $" OTG_HS_HOST_OTG_HS_HCINT5 @ hex. OTG_HS_HOST_OTG_HS_HCINT5 1b. ;
    : OTG_HS_HOST_OTG_HS_HCINT6. cr ." OTG_HS_HOST_OTG_HS_HCINT6.  RW   $" OTG_HS_HOST_OTG_HS_HCINT6 @ hex. OTG_HS_HOST_OTG_HS_HCINT6 1b. ;
    : OTG_HS_HOST_OTG_HS_HCINT7. cr ." OTG_HS_HOST_OTG_HS_HCINT7.  RW   $" OTG_HS_HOST_OTG_HS_HCINT7 @ hex. OTG_HS_HOST_OTG_HS_HCINT7 1b. ;
    : OTG_HS_HOST_OTG_HS_HCINT8. cr ." OTG_HS_HOST_OTG_HS_HCINT8.  RW   $" OTG_HS_HOST_OTG_HS_HCINT8 @ hex. OTG_HS_HOST_OTG_HS_HCINT8 1b. ;
    : OTG_HS_HOST_OTG_HS_HCINT9. cr ." OTG_HS_HOST_OTG_HS_HCINT9.  RW   $" OTG_HS_HOST_OTG_HS_HCINT9 @ hex. OTG_HS_HOST_OTG_HS_HCINT9 1b. ;
    : OTG_HS_HOST_OTG_HS_HCINT10. cr ." OTG_HS_HOST_OTG_HS_HCINT10.  RW   $" OTG_HS_HOST_OTG_HS_HCINT10 @ hex. OTG_HS_HOST_OTG_HS_HCINT10 1b. ;
    : OTG_HS_HOST_OTG_HS_HCINT11. cr ." OTG_HS_HOST_OTG_HS_HCINT11.  RW   $" OTG_HS_HOST_OTG_HS_HCINT11 @ hex. OTG_HS_HOST_OTG_HS_HCINT11 1b. ;
    : OTG_HS_HOST_OTG_HS_HCINTMSK0. cr ." OTG_HS_HOST_OTG_HS_HCINTMSK0.  RW   $" OTG_HS_HOST_OTG_HS_HCINTMSK0 @ hex. OTG_HS_HOST_OTG_HS_HCINTMSK0 1b. ;
    : OTG_HS_HOST_OTG_HS_HCINTMSK1. cr ." OTG_HS_HOST_OTG_HS_HCINTMSK1.  RW   $" OTG_HS_HOST_OTG_HS_HCINTMSK1 @ hex. OTG_HS_HOST_OTG_HS_HCINTMSK1 1b. ;
    : OTG_HS_HOST_OTG_HS_HCINTMSK2. cr ." OTG_HS_HOST_OTG_HS_HCINTMSK2.  RW   $" OTG_HS_HOST_OTG_HS_HCINTMSK2 @ hex. OTG_HS_HOST_OTG_HS_HCINTMSK2 1b. ;
    : OTG_HS_HOST_OTG_HS_HCINTMSK3. cr ." OTG_HS_HOST_OTG_HS_HCINTMSK3.  RW   $" OTG_HS_HOST_OTG_HS_HCINTMSK3 @ hex. OTG_HS_HOST_OTG_HS_HCINTMSK3 1b. ;
    : OTG_HS_HOST_OTG_HS_HCINTMSK4. cr ." OTG_HS_HOST_OTG_HS_HCINTMSK4.  RW   $" OTG_HS_HOST_OTG_HS_HCINTMSK4 @ hex. OTG_HS_HOST_OTG_HS_HCINTMSK4 1b. ;
    : OTG_HS_HOST_OTG_HS_HCINTMSK5. cr ." OTG_HS_HOST_OTG_HS_HCINTMSK5.  RW   $" OTG_HS_HOST_OTG_HS_HCINTMSK5 @ hex. OTG_HS_HOST_OTG_HS_HCINTMSK5 1b. ;
    : OTG_HS_HOST_OTG_HS_HCINTMSK6. cr ." OTG_HS_HOST_OTG_HS_HCINTMSK6.  RW   $" OTG_HS_HOST_OTG_HS_HCINTMSK6 @ hex. OTG_HS_HOST_OTG_HS_HCINTMSK6 1b. ;
    : OTG_HS_HOST_OTG_HS_HCINTMSK7. cr ." OTG_HS_HOST_OTG_HS_HCINTMSK7.  RW   $" OTG_HS_HOST_OTG_HS_HCINTMSK7 @ hex. OTG_HS_HOST_OTG_HS_HCINTMSK7 1b. ;
    : OTG_HS_HOST_OTG_HS_HCINTMSK8. cr ." OTG_HS_HOST_OTG_HS_HCINTMSK8.  RW   $" OTG_HS_HOST_OTG_HS_HCINTMSK8 @ hex. OTG_HS_HOST_OTG_HS_HCINTMSK8 1b. ;
    : OTG_HS_HOST_OTG_HS_HCINTMSK9. cr ." OTG_HS_HOST_OTG_HS_HCINTMSK9.  RW   $" OTG_HS_HOST_OTG_HS_HCINTMSK9 @ hex. OTG_HS_HOST_OTG_HS_HCINTMSK9 1b. ;
    : OTG_HS_HOST_OTG_HS_HCINTMSK10. cr ." OTG_HS_HOST_OTG_HS_HCINTMSK10.  RW   $" OTG_HS_HOST_OTG_HS_HCINTMSK10 @ hex. OTG_HS_HOST_OTG_HS_HCINTMSK10 1b. ;
    : OTG_HS_HOST_OTG_HS_HCINTMSK11. cr ." OTG_HS_HOST_OTG_HS_HCINTMSK11.  RW   $" OTG_HS_HOST_OTG_HS_HCINTMSK11 @ hex. OTG_HS_HOST_OTG_HS_HCINTMSK11 1b. ;
    : OTG_HS_HOST_OTG_HS_HCTSIZ0. cr ." OTG_HS_HOST_OTG_HS_HCTSIZ0.  RW   $" OTG_HS_HOST_OTG_HS_HCTSIZ0 @ hex. OTG_HS_HOST_OTG_HS_HCTSIZ0 1b. ;
    : OTG_HS_HOST_OTG_HS_HCTSIZ1. cr ." OTG_HS_HOST_OTG_HS_HCTSIZ1.  RW   $" OTG_HS_HOST_OTG_HS_HCTSIZ1 @ hex. OTG_HS_HOST_OTG_HS_HCTSIZ1 1b. ;
    : OTG_HS_HOST_OTG_HS_HCTSIZ2. cr ." OTG_HS_HOST_OTG_HS_HCTSIZ2.  RW   $" OTG_HS_HOST_OTG_HS_HCTSIZ2 @ hex. OTG_HS_HOST_OTG_HS_HCTSIZ2 1b. ;
    : OTG_HS_HOST_OTG_HS_HCTSIZ3. cr ." OTG_HS_HOST_OTG_HS_HCTSIZ3.  RW   $" OTG_HS_HOST_OTG_HS_HCTSIZ3 @ hex. OTG_HS_HOST_OTG_HS_HCTSIZ3 1b. ;
    : OTG_HS_HOST_OTG_HS_HCTSIZ4. cr ." OTG_HS_HOST_OTG_HS_HCTSIZ4.  RW   $" OTG_HS_HOST_OTG_HS_HCTSIZ4 @ hex. OTG_HS_HOST_OTG_HS_HCTSIZ4 1b. ;
    : OTG_HS_HOST_OTG_HS_HCTSIZ5. cr ." OTG_HS_HOST_OTG_HS_HCTSIZ5.  RW   $" OTG_HS_HOST_OTG_HS_HCTSIZ5 @ hex. OTG_HS_HOST_OTG_HS_HCTSIZ5 1b. ;
    : OTG_HS_HOST_OTG_HS_HCTSIZ6. cr ." OTG_HS_HOST_OTG_HS_HCTSIZ6.  RW   $" OTG_HS_HOST_OTG_HS_HCTSIZ6 @ hex. OTG_HS_HOST_OTG_HS_HCTSIZ6 1b. ;
    : OTG_HS_HOST_OTG_HS_HCTSIZ7. cr ." OTG_HS_HOST_OTG_HS_HCTSIZ7.  RW   $" OTG_HS_HOST_OTG_HS_HCTSIZ7 @ hex. OTG_HS_HOST_OTG_HS_HCTSIZ7 1b. ;
    : OTG_HS_HOST_OTG_HS_HCTSIZ8. cr ." OTG_HS_HOST_OTG_HS_HCTSIZ8.  RW   $" OTG_HS_HOST_OTG_HS_HCTSIZ8 @ hex. OTG_HS_HOST_OTG_HS_HCTSIZ8 1b. ;
    : OTG_HS_HOST_OTG_HS_HCTSIZ9. cr ." OTG_HS_HOST_OTG_HS_HCTSIZ9.  RW   $" OTG_HS_HOST_OTG_HS_HCTSIZ9 @ hex. OTG_HS_HOST_OTG_HS_HCTSIZ9 1b. ;
    : OTG_HS_HOST_OTG_HS_HCTSIZ10. cr ." OTG_HS_HOST_OTG_HS_HCTSIZ10.  RW   $" OTG_HS_HOST_OTG_HS_HCTSIZ10 @ hex. OTG_HS_HOST_OTG_HS_HCTSIZ10 1b. ;
    : OTG_HS_HOST_OTG_HS_HCTSIZ11. cr ." OTG_HS_HOST_OTG_HS_HCTSIZ11.  RW   $" OTG_HS_HOST_OTG_HS_HCTSIZ11 @ hex. OTG_HS_HOST_OTG_HS_HCTSIZ11 1b. ;
    : OTG_HS_HOST_OTG_HS_HCDMA0. cr ." OTG_HS_HOST_OTG_HS_HCDMA0.  RW   $" OTG_HS_HOST_OTG_HS_HCDMA0 @ hex. OTG_HS_HOST_OTG_HS_HCDMA0 1b. ;
    : OTG_HS_HOST_OTG_HS_HCDMA1. cr ." OTG_HS_HOST_OTG_HS_HCDMA1.  RW   $" OTG_HS_HOST_OTG_HS_HCDMA1 @ hex. OTG_HS_HOST_OTG_HS_HCDMA1 1b. ;
    : OTG_HS_HOST_OTG_HS_HCDMA2. cr ." OTG_HS_HOST_OTG_HS_HCDMA2.  RW   $" OTG_HS_HOST_OTG_HS_HCDMA2 @ hex. OTG_HS_HOST_OTG_HS_HCDMA2 1b. ;
    : OTG_HS_HOST_OTG_HS_HCDMA3. cr ." OTG_HS_HOST_OTG_HS_HCDMA3.  RW   $" OTG_HS_HOST_OTG_HS_HCDMA3 @ hex. OTG_HS_HOST_OTG_HS_HCDMA3 1b. ;
    : OTG_HS_HOST_OTG_HS_HCDMA4. cr ." OTG_HS_HOST_OTG_HS_HCDMA4.  RW   $" OTG_HS_HOST_OTG_HS_HCDMA4 @ hex. OTG_HS_HOST_OTG_HS_HCDMA4 1b. ;
    : OTG_HS_HOST_OTG_HS_HCDMA5. cr ." OTG_HS_HOST_OTG_HS_HCDMA5.  RW   $" OTG_HS_HOST_OTG_HS_HCDMA5 @ hex. OTG_HS_HOST_OTG_HS_HCDMA5 1b. ;
    : OTG_HS_HOST_OTG_HS_HCDMA6. cr ." OTG_HS_HOST_OTG_HS_HCDMA6.  RW   $" OTG_HS_HOST_OTG_HS_HCDMA6 @ hex. OTG_HS_HOST_OTG_HS_HCDMA6 1b. ;
    : OTG_HS_HOST_OTG_HS_HCDMA7. cr ." OTG_HS_HOST_OTG_HS_HCDMA7.  RW   $" OTG_HS_HOST_OTG_HS_HCDMA7 @ hex. OTG_HS_HOST_OTG_HS_HCDMA7 1b. ;
    : OTG_HS_HOST_OTG_HS_HCDMA8. cr ." OTG_HS_HOST_OTG_HS_HCDMA8.  RW   $" OTG_HS_HOST_OTG_HS_HCDMA8 @ hex. OTG_HS_HOST_OTG_HS_HCDMA8 1b. ;
    : OTG_HS_HOST_OTG_HS_HCDMA9. cr ." OTG_HS_HOST_OTG_HS_HCDMA9.  RW   $" OTG_HS_HOST_OTG_HS_HCDMA9 @ hex. OTG_HS_HOST_OTG_HS_HCDMA9 1b. ;
    : OTG_HS_HOST_OTG_HS_HCDMA10. cr ." OTG_HS_HOST_OTG_HS_HCDMA10.  RW   $" OTG_HS_HOST_OTG_HS_HCDMA10 @ hex. OTG_HS_HOST_OTG_HS_HCDMA10 1b. ;
    : OTG_HS_HOST_OTG_HS_HCDMA11. cr ." OTG_HS_HOST_OTG_HS_HCDMA11.  RW   $" OTG_HS_HOST_OTG_HS_HCDMA11 @ hex. OTG_HS_HOST_OTG_HS_HCDMA11 1b. ;
    : OTG_HS_HOST_OTG_HS_HCCHAR12. cr ." OTG_HS_HOST_OTG_HS_HCCHAR12.  RW   $" OTG_HS_HOST_OTG_HS_HCCHAR12 @ hex. OTG_HS_HOST_OTG_HS_HCCHAR12 1b. ;
    : OTG_HS_HOST_OTG_HS_HCSPLT12. cr ." OTG_HS_HOST_OTG_HS_HCSPLT12.  RW   $" OTG_HS_HOST_OTG_HS_HCSPLT12 @ hex. OTG_HS_HOST_OTG_HS_HCSPLT12 1b. ;
    : OTG_HS_HOST_OTG_HS_HCINT12. cr ." OTG_HS_HOST_OTG_HS_HCINT12.  RW   $" OTG_HS_HOST_OTG_HS_HCINT12 @ hex. OTG_HS_HOST_OTG_HS_HCINT12 1b. ;
    : OTG_HS_HOST_OTG_HS_HCINTMSK12. cr ." OTG_HS_HOST_OTG_HS_HCINTMSK12.  RW   $" OTG_HS_HOST_OTG_HS_HCINTMSK12 @ hex. OTG_HS_HOST_OTG_HS_HCINTMSK12 1b. ;
    : OTG_HS_HOST_OTG_HS_HCTSIZ12. cr ." OTG_HS_HOST_OTG_HS_HCTSIZ12.  RW   $" OTG_HS_HOST_OTG_HS_HCTSIZ12 @ hex. OTG_HS_HOST_OTG_HS_HCTSIZ12 1b. ;
    : OTG_HS_HOST_OTG_HS_HCDMA12. cr ." OTG_HS_HOST_OTG_HS_HCDMA12.  RW   $" OTG_HS_HOST_OTG_HS_HCDMA12 @ hex. OTG_HS_HOST_OTG_HS_HCDMA12 1b. ;
    : OTG_HS_HOST_OTG_HS_HCCHAR13. cr ." OTG_HS_HOST_OTG_HS_HCCHAR13.  RW   $" OTG_HS_HOST_OTG_HS_HCCHAR13 @ hex. OTG_HS_HOST_OTG_HS_HCCHAR13 1b. ;
    : OTG_HS_HOST_OTG_HS_HCSPLT13. cr ." OTG_HS_HOST_OTG_HS_HCSPLT13.  RW   $" OTG_HS_HOST_OTG_HS_HCSPLT13 @ hex. OTG_HS_HOST_OTG_HS_HCSPLT13 1b. ;
    : OTG_HS_HOST_OTG_HS_HCINT13. cr ." OTG_HS_HOST_OTG_HS_HCINT13.  RW   $" OTG_HS_HOST_OTG_HS_HCINT13 @ hex. OTG_HS_HOST_OTG_HS_HCINT13 1b. ;
    : OTG_HS_HOST_OTG_HS_HCINTMSK13. cr ." OTG_HS_HOST_OTG_HS_HCINTMSK13.  RW   $" OTG_HS_HOST_OTG_HS_HCINTMSK13 @ hex. OTG_HS_HOST_OTG_HS_HCINTMSK13 1b. ;
    : OTG_HS_HOST_OTG_HS_HCTSIZ13. cr ." OTG_HS_HOST_OTG_HS_HCTSIZ13.  RW   $" OTG_HS_HOST_OTG_HS_HCTSIZ13 @ hex. OTG_HS_HOST_OTG_HS_HCTSIZ13 1b. ;
    : OTG_HS_HOST_OTG_HS_HCDMA13. cr ." OTG_HS_HOST_OTG_HS_HCDMA13.  RW   $" OTG_HS_HOST_OTG_HS_HCDMA13 @ hex. OTG_HS_HOST_OTG_HS_HCDMA13 1b. ;
    : OTG_HS_HOST_OTG_HS_HCCHAR14. cr ." OTG_HS_HOST_OTG_HS_HCCHAR14.  RW   $" OTG_HS_HOST_OTG_HS_HCCHAR14 @ hex. OTG_HS_HOST_OTG_HS_HCCHAR14 1b. ;
    : OTG_HS_HOST_OTG_HS_HCSPLT14. cr ." OTG_HS_HOST_OTG_HS_HCSPLT14.  RW   $" OTG_HS_HOST_OTG_HS_HCSPLT14 @ hex. OTG_HS_HOST_OTG_HS_HCSPLT14 1b. ;
    : OTG_HS_HOST_OTG_HS_HCINT14. cr ." OTG_HS_HOST_OTG_HS_HCINT14.  RW   $" OTG_HS_HOST_OTG_HS_HCINT14 @ hex. OTG_HS_HOST_OTG_HS_HCINT14 1b. ;
    : OTG_HS_HOST_OTG_HS_HCINTMSK14. cr ." OTG_HS_HOST_OTG_HS_HCINTMSK14.  RW   $" OTG_HS_HOST_OTG_HS_HCINTMSK14 @ hex. OTG_HS_HOST_OTG_HS_HCINTMSK14 1b. ;
    : OTG_HS_HOST_OTG_HS_HCTSIZ14. cr ." OTG_HS_HOST_OTG_HS_HCTSIZ14.  RW   $" OTG_HS_HOST_OTG_HS_HCTSIZ14 @ hex. OTG_HS_HOST_OTG_HS_HCTSIZ14 1b. ;
    : OTG_HS_HOST_OTG_HS_HCDMA14. cr ." OTG_HS_HOST_OTG_HS_HCDMA14.  RW   $" OTG_HS_HOST_OTG_HS_HCDMA14 @ hex. OTG_HS_HOST_OTG_HS_HCDMA14 1b. ;
    : OTG_HS_HOST_OTG_HS_HCCHAR15. cr ." OTG_HS_HOST_OTG_HS_HCCHAR15.  RW   $" OTG_HS_HOST_OTG_HS_HCCHAR15 @ hex. OTG_HS_HOST_OTG_HS_HCCHAR15 1b. ;
    : OTG_HS_HOST_OTG_HS_HCSPLT15. cr ." OTG_HS_HOST_OTG_HS_HCSPLT15.  RW   $" OTG_HS_HOST_OTG_HS_HCSPLT15 @ hex. OTG_HS_HOST_OTG_HS_HCSPLT15 1b. ;
    : OTG_HS_HOST_OTG_HS_HCINT15. cr ." OTG_HS_HOST_OTG_HS_HCINT15.  RW   $" OTG_HS_HOST_OTG_HS_HCINT15 @ hex. OTG_HS_HOST_OTG_HS_HCINT15 1b. ;
    : OTG_HS_HOST_OTG_HS_HCINTMSK15. cr ." OTG_HS_HOST_OTG_HS_HCINTMSK15.  RW   $" OTG_HS_HOST_OTG_HS_HCINTMSK15 @ hex. OTG_HS_HOST_OTG_HS_HCINTMSK15 1b. ;
    : OTG_HS_HOST_OTG_HS_HCTSIZ15. cr ." OTG_HS_HOST_OTG_HS_HCTSIZ15.  RW   $" OTG_HS_HOST_OTG_HS_HCTSIZ15 @ hex. OTG_HS_HOST_OTG_HS_HCTSIZ15 1b. ;
    : OTG_HS_HOST_OTG_HS_HCDMA15. cr ." OTG_HS_HOST_OTG_HS_HCDMA15.  RW   $" OTG_HS_HOST_OTG_HS_HCDMA15 @ hex. OTG_HS_HOST_OTG_HS_HCDMA15 1b. ;
    : OTG_HS_HOST.
      OTG_HS_HOST_OTG_HS_HCFG.
      OTG_HS_HOST_OTG_HS_HFIR.
      OTG_HS_HOST_OTG_HS_HFNUM.
      OTG_HS_HOST_OTG_HS_HPTXSTS.
      OTG_HS_HOST_OTG_HS_HAINT.
      OTG_HS_HOST_OTG_HS_HAINTMSK.
      OTG_HS_HOST_OTG_HS_HPRT.
      OTG_HS_HOST_OTG_HS_HCCHAR0.
      OTG_HS_HOST_OTG_HS_HCCHAR1.
      OTG_HS_HOST_OTG_HS_HCCHAR2.
      OTG_HS_HOST_OTG_HS_HCCHAR3.
      OTG_HS_HOST_OTG_HS_HCCHAR4.
      OTG_HS_HOST_OTG_HS_HCCHAR5.
      OTG_HS_HOST_OTG_HS_HCCHAR6.
      OTG_HS_HOST_OTG_HS_HCCHAR7.
      OTG_HS_HOST_OTG_HS_HCCHAR8.
      OTG_HS_HOST_OTG_HS_HCCHAR9.
      OTG_HS_HOST_OTG_HS_HCCHAR10.
      OTG_HS_HOST_OTG_HS_HCCHAR11.
      OTG_HS_HOST_OTG_HS_HCSPLT0.
      OTG_HS_HOST_OTG_HS_HCSPLT1.
      OTG_HS_HOST_OTG_HS_HCSPLT2.
      OTG_HS_HOST_OTG_HS_HCSPLT3.
      OTG_HS_HOST_OTG_HS_HCSPLT4.
      OTG_HS_HOST_OTG_HS_HCSPLT5.
      OTG_HS_HOST_OTG_HS_HCSPLT6.
      OTG_HS_HOST_OTG_HS_HCSPLT7.
      OTG_HS_HOST_OTG_HS_HCSPLT8.
      OTG_HS_HOST_OTG_HS_HCSPLT9.
      OTG_HS_HOST_OTG_HS_HCSPLT10.
      OTG_HS_HOST_OTG_HS_HCSPLT11.
      OTG_HS_HOST_OTG_HS_HCINT0.
      OTG_HS_HOST_OTG_HS_HCINT1.
      OTG_HS_HOST_OTG_HS_HCINT2.
      OTG_HS_HOST_OTG_HS_HCINT3.
      OTG_HS_HOST_OTG_HS_HCINT4.
      OTG_HS_HOST_OTG_HS_HCINT5.
      OTG_HS_HOST_OTG_HS_HCINT6.
      OTG_HS_HOST_OTG_HS_HCINT7.
      OTG_HS_HOST_OTG_HS_HCINT8.
      OTG_HS_HOST_OTG_HS_HCINT9.
      OTG_HS_HOST_OTG_HS_HCINT10.
      OTG_HS_HOST_OTG_HS_HCINT11.
      OTG_HS_HOST_OTG_HS_HCINTMSK0.
      OTG_HS_HOST_OTG_HS_HCINTMSK1.
      OTG_HS_HOST_OTG_HS_HCINTMSK2.
      OTG_HS_HOST_OTG_HS_HCINTMSK3.
      OTG_HS_HOST_OTG_HS_HCINTMSK4.
      OTG_HS_HOST_OTG_HS_HCINTMSK5.
      OTG_HS_HOST_OTG_HS_HCINTMSK6.
      OTG_HS_HOST_OTG_HS_HCINTMSK7.
      OTG_HS_HOST_OTG_HS_HCINTMSK8.
      OTG_HS_HOST_OTG_HS_HCINTMSK9.
      OTG_HS_HOST_OTG_HS_HCINTMSK10.
      OTG_HS_HOST_OTG_HS_HCINTMSK11.
      OTG_HS_HOST_OTG_HS_HCTSIZ0.
      OTG_HS_HOST_OTG_HS_HCTSIZ1.
      OTG_HS_HOST_OTG_HS_HCTSIZ2.
      OTG_HS_HOST_OTG_HS_HCTSIZ3.
      OTG_HS_HOST_OTG_HS_HCTSIZ4.
      OTG_HS_HOST_OTG_HS_HCTSIZ5.
      OTG_HS_HOST_OTG_HS_HCTSIZ6.
      OTG_HS_HOST_OTG_HS_HCTSIZ7.
      OTG_HS_HOST_OTG_HS_HCTSIZ8.
      OTG_HS_HOST_OTG_HS_HCTSIZ9.
      OTG_HS_HOST_OTG_HS_HCTSIZ10.
      OTG_HS_HOST_OTG_HS_HCTSIZ11.
      OTG_HS_HOST_OTG_HS_HCDMA0.
      OTG_HS_HOST_OTG_HS_HCDMA1.
      OTG_HS_HOST_OTG_HS_HCDMA2.
      OTG_HS_HOST_OTG_HS_HCDMA3.
      OTG_HS_HOST_OTG_HS_HCDMA4.
      OTG_HS_HOST_OTG_HS_HCDMA5.
      OTG_HS_HOST_OTG_HS_HCDMA6.
      OTG_HS_HOST_OTG_HS_HCDMA7.
      OTG_HS_HOST_OTG_HS_HCDMA8.
      OTG_HS_HOST_OTG_HS_HCDMA9.
      OTG_HS_HOST_OTG_HS_HCDMA10.
      OTG_HS_HOST_OTG_HS_HCDMA11.
      OTG_HS_HOST_OTG_HS_HCCHAR12.
      OTG_HS_HOST_OTG_HS_HCSPLT12.
      OTG_HS_HOST_OTG_HS_HCINT12.
      OTG_HS_HOST_OTG_HS_HCINTMSK12.
      OTG_HS_HOST_OTG_HS_HCTSIZ12.
      OTG_HS_HOST_OTG_HS_HCDMA12.
      OTG_HS_HOST_OTG_HS_HCCHAR13.
      OTG_HS_HOST_OTG_HS_HCSPLT13.
      OTG_HS_HOST_OTG_HS_HCINT13.
      OTG_HS_HOST_OTG_HS_HCINTMSK13.
      OTG_HS_HOST_OTG_HS_HCTSIZ13.
      OTG_HS_HOST_OTG_HS_HCDMA13.
      OTG_HS_HOST_OTG_HS_HCCHAR14.
      OTG_HS_HOST_OTG_HS_HCSPLT14.
      OTG_HS_HOST_OTG_HS_HCINT14.
      OTG_HS_HOST_OTG_HS_HCINTMSK14.
      OTG_HS_HOST_OTG_HS_HCTSIZ14.
      OTG_HS_HOST_OTG_HS_HCDMA14.
      OTG_HS_HOST_OTG_HS_HCCHAR15.
      OTG_HS_HOST_OTG_HS_HCSPLT15.
      OTG_HS_HOST_OTG_HS_HCINT15.
      OTG_HS_HOST_OTG_HS_HCINTMSK15.
      OTG_HS_HOST_OTG_HS_HCTSIZ15.
      OTG_HS_HOST_OTG_HS_HCDMA15.
    ;
  [then]

  execute-defined? use-OTG_HS_DEVICE [if]
    $40040800 constant OTG_HS_DEVICE ( USB on the go high speed ) 
    OTG_HS_DEVICE $0 + constant OTG_HS_DEVICE_OTG_HS_DCFG ( read-write )  \ OTG_HS device configuration register
    OTG_HS_DEVICE $4 + constant OTG_HS_DEVICE_OTG_HS_DCTL (  )  \ OTG_HS device control register
    OTG_HS_DEVICE $8 + constant OTG_HS_DEVICE_OTG_HS_DSTS ( read-only )  \ OTG_HS device status register
    OTG_HS_DEVICE $10 + constant OTG_HS_DEVICE_OTG_HS_DIEPMSK ( read-write )  \ OTG_HS device IN endpoint common interrupt mask register
    OTG_HS_DEVICE $14 + constant OTG_HS_DEVICE_OTG_HS_DOEPMSK ( read-write )  \ OTG_HS device OUT endpoint common interrupt mask register
    OTG_HS_DEVICE $18 + constant OTG_HS_DEVICE_OTG_HS_DAINT ( read-only )  \ OTG_HS device all endpoints interrupt register
    OTG_HS_DEVICE $1C + constant OTG_HS_DEVICE_OTG_HS_DAINTMSK ( read-write )  \ OTG_HS all endpoints interrupt mask register
    OTG_HS_DEVICE $28 + constant OTG_HS_DEVICE_OTG_HS_DVBUSDIS ( read-write )  \ OTG_HS device VBUS discharge time register
    OTG_HS_DEVICE $2C + constant OTG_HS_DEVICE_OTG_HS_DVBUSPULSE ( read-write )  \ OTG_HS device VBUS pulsing time register
    OTG_HS_DEVICE $30 + constant OTG_HS_DEVICE_OTG_HS_DTHRCTL ( read-write )  \ OTG_HS Device threshold control register
    OTG_HS_DEVICE $34 + constant OTG_HS_DEVICE_OTG_HS_DIEPEMPMSK ( read-write )  \ OTG_HS device IN endpoint FIFO empty interrupt mask register
    OTG_HS_DEVICE $38 + constant OTG_HS_DEVICE_OTG_HS_DEACHINT ( read-write )  \ OTG_HS device each endpoint interrupt register
    OTG_HS_DEVICE $3C + constant OTG_HS_DEVICE_OTG_HS_DEACHINTMSK ( read-write )  \ OTG_HS device each endpoint interrupt register mask
    OTG_HS_DEVICE $100 + constant OTG_HS_DEVICE_OTG_HS_DIEPCTL0 (  )  \ OTG device endpoint-0 control register
    OTG_HS_DEVICE $120 + constant OTG_HS_DEVICE_OTG_HS_DIEPCTL1 (  )  \ OTG device endpoint-1 control register
    OTG_HS_DEVICE $140 + constant OTG_HS_DEVICE_OTG_HS_DIEPCTL2 (  )  \ OTG device endpoint-2 control register
    OTG_HS_DEVICE $160 + constant OTG_HS_DEVICE_OTG_HS_DIEPCTL3 (  )  \ OTG device endpoint-3 control register
    OTG_HS_DEVICE $180 + constant OTG_HS_DEVICE_OTG_HS_DIEPCTL4 (  )  \ OTG device endpoint-4 control register
    OTG_HS_DEVICE $1A0 + constant OTG_HS_DEVICE_OTG_HS_DIEPCTL5 (  )  \ OTG device endpoint-5 control register
    OTG_HS_DEVICE $1C0 + constant OTG_HS_DEVICE_OTG_HS_DIEPCTL6 (  )  \ OTG device endpoint-6 control register
    OTG_HS_DEVICE $1E0 + constant OTG_HS_DEVICE_OTG_HS_DIEPCTL7 (  )  \ OTG device endpoint-7 control register
    OTG_HS_DEVICE $108 + constant OTG_HS_DEVICE_OTG_HS_DIEPINT0 (  )  \ OTG device endpoint-0 interrupt register
    OTG_HS_DEVICE $128 + constant OTG_HS_DEVICE_OTG_HS_DIEPINT1 (  )  \ OTG device endpoint-1 interrupt register
    OTG_HS_DEVICE $148 + constant OTG_HS_DEVICE_OTG_HS_DIEPINT2 (  )  \ OTG device endpoint-2 interrupt register
    OTG_HS_DEVICE $168 + constant OTG_HS_DEVICE_OTG_HS_DIEPINT3 (  )  \ OTG device endpoint-3 interrupt register
    OTG_HS_DEVICE $188 + constant OTG_HS_DEVICE_OTG_HS_DIEPINT4 (  )  \ OTG device endpoint-4 interrupt register
    OTG_HS_DEVICE $1A8 + constant OTG_HS_DEVICE_OTG_HS_DIEPINT5 (  )  \ OTG device endpoint-5 interrupt register
    OTG_HS_DEVICE $1C8 + constant OTG_HS_DEVICE_OTG_HS_DIEPINT6 (  )  \ OTG device endpoint-6 interrupt register
    OTG_HS_DEVICE $1E8 + constant OTG_HS_DEVICE_OTG_HS_DIEPINT7 (  )  \ OTG device endpoint-7 interrupt register
    OTG_HS_DEVICE $110 + constant OTG_HS_DEVICE_OTG_HS_DIEPTSIZ0 ( read-write )  \ OTG_HS device IN endpoint 0 transfer size register
    OTG_HS_DEVICE $114 + constant OTG_HS_DEVICE_OTG_HS_DIEPDMA1 ( read-write )  \ OTG_HS device endpoint-1 DMA address register
    OTG_HS_DEVICE $134 + constant OTG_HS_DEVICE_OTG_HS_DIEPDMA2 ( read-write )  \ OTG_HS device endpoint-2 DMA address register
    OTG_HS_DEVICE $154 + constant OTG_HS_DEVICE_OTG_HS_DIEPDMA3 ( read-write )  \ OTG_HS device endpoint-3 DMA address register
    OTG_HS_DEVICE $174 + constant OTG_HS_DEVICE_OTG_HS_DIEPDMA4 ( read-write )  \ OTG_HS device endpoint-4 DMA address register
    OTG_HS_DEVICE $194 + constant OTG_HS_DEVICE_OTG_HS_DIEPDMA5 ( read-write )  \ OTG_HS device endpoint-5 DMA address register
    OTG_HS_DEVICE $118 + constant OTG_HS_DEVICE_OTG_HS_DTXFSTS0 ( read-only )  \ OTG_HS device IN endpoint transmit FIFO status register
    OTG_HS_DEVICE $138 + constant OTG_HS_DEVICE_OTG_HS_DTXFSTS1 ( read-only )  \ OTG_HS device IN endpoint transmit FIFO status register
    OTG_HS_DEVICE $158 + constant OTG_HS_DEVICE_OTG_HS_DTXFSTS2 ( read-only )  \ OTG_HS device IN endpoint transmit FIFO status register
    OTG_HS_DEVICE $178 + constant OTG_HS_DEVICE_OTG_HS_DTXFSTS3 ( read-only )  \ OTG_HS device IN endpoint transmit FIFO status register
    OTG_HS_DEVICE $198 + constant OTG_HS_DEVICE_OTG_HS_DTXFSTS4 ( read-only )  \ OTG_HS device IN endpoint transmit FIFO status register
    OTG_HS_DEVICE $1B8 + constant OTG_HS_DEVICE_OTG_HS_DTXFSTS5 ( read-only )  \ OTG_HS device IN endpoint transmit FIFO status register
    OTG_HS_DEVICE $130 + constant OTG_HS_DEVICE_OTG_HS_DIEPTSIZ1 ( read-write )  \ OTG_HS device endpoint transfer size register
    OTG_HS_DEVICE $150 + constant OTG_HS_DEVICE_OTG_HS_DIEPTSIZ2 ( read-write )  \ OTG_HS device endpoint transfer size register
    OTG_HS_DEVICE $170 + constant OTG_HS_DEVICE_OTG_HS_DIEPTSIZ3 ( read-write )  \ OTG_HS device endpoint transfer size register
    OTG_HS_DEVICE $190 + constant OTG_HS_DEVICE_OTG_HS_DIEPTSIZ4 ( read-write )  \ OTG_HS device endpoint transfer size register
    OTG_HS_DEVICE $1B0 + constant OTG_HS_DEVICE_OTG_HS_DIEPTSIZ5 ( read-write )  \ OTG_HS device endpoint transfer size register
    OTG_HS_DEVICE $300 + constant OTG_HS_DEVICE_OTG_HS_DOEPCTL0 (  )  \ OTG_HS device control OUT endpoint 0 control register
    OTG_HS_DEVICE $320 + constant OTG_HS_DEVICE_OTG_HS_DOEPCTL1 (  )  \ OTG device endpoint-1 control register
    OTG_HS_DEVICE $340 + constant OTG_HS_DEVICE_OTG_HS_DOEPCTL2 (  )  \ OTG device endpoint-2 control register
    OTG_HS_DEVICE $360 + constant OTG_HS_DEVICE_OTG_HS_DOEPCTL3 (  )  \ OTG device endpoint-3 control register
    OTG_HS_DEVICE $308 + constant OTG_HS_DEVICE_OTG_HS_DOEPINT0 ( read-write )  \ OTG_HS device endpoint-0 interrupt register
    OTG_HS_DEVICE $328 + constant OTG_HS_DEVICE_OTG_HS_DOEPINT1 ( read-write )  \ OTG_HS device endpoint-1 interrupt register
    OTG_HS_DEVICE $348 + constant OTG_HS_DEVICE_OTG_HS_DOEPINT2 ( read-write )  \ OTG_HS device endpoint-2 interrupt register
    OTG_HS_DEVICE $368 + constant OTG_HS_DEVICE_OTG_HS_DOEPINT3 ( read-write )  \ OTG_HS device endpoint-3 interrupt register
    OTG_HS_DEVICE $388 + constant OTG_HS_DEVICE_OTG_HS_DOEPINT4 ( read-write )  \ OTG_HS device endpoint-4 interrupt register
    OTG_HS_DEVICE $3A8 + constant OTG_HS_DEVICE_OTG_HS_DOEPINT5 ( read-write )  \ OTG_HS device endpoint-5 interrupt register
    OTG_HS_DEVICE $3C8 + constant OTG_HS_DEVICE_OTG_HS_DOEPINT6 ( read-write )  \ OTG_HS device endpoint-6 interrupt register
    OTG_HS_DEVICE $3E8 + constant OTG_HS_DEVICE_OTG_HS_DOEPINT7 ( read-write )  \ OTG_HS device endpoint-7 interrupt register
    OTG_HS_DEVICE $310 + constant OTG_HS_DEVICE_OTG_HS_DOEPTSIZ0 ( read-write )  \ OTG_HS device endpoint-0 transfer size register
    OTG_HS_DEVICE $330 + constant OTG_HS_DEVICE_OTG_HS_DOEPTSIZ1 ( read-write )  \ OTG_HS device endpoint-1 transfer size register
    OTG_HS_DEVICE $350 + constant OTG_HS_DEVICE_OTG_HS_DOEPTSIZ2 ( read-write )  \ OTG_HS device endpoint-2 transfer size register
    OTG_HS_DEVICE $370 + constant OTG_HS_DEVICE_OTG_HS_DOEPTSIZ3 ( read-write )  \ OTG_HS device endpoint-3 transfer size register
    OTG_HS_DEVICE $390 + constant OTG_HS_DEVICE_OTG_HS_DOEPTSIZ4 ( read-write )  \ OTG_HS device endpoint-4 transfer size register
    OTG_HS_DEVICE $1A0 + constant OTG_HS_DEVICE_OTG_HS_DIEPTSIZ6 ( read-write )  \ OTG_HS device endpoint transfer size register
    OTG_HS_DEVICE $1A4 + constant OTG_HS_DEVICE_OTG_HS_DTXFSTS6 ( read-write )  \ OTG_HS device IN endpoint transmit FIFO status register
    OTG_HS_DEVICE $1A8 + constant OTG_HS_DEVICE_OTG_HS_DIEPTSIZ7 ( read-write )  \ OTG_HS device endpoint transfer size register
    OTG_HS_DEVICE $1AC + constant OTG_HS_DEVICE_OTG_HS_DTXFSTS7 ( read-write )  \ OTG_HS device IN endpoint transmit FIFO status register
    OTG_HS_DEVICE $380 + constant OTG_HS_DEVICE_OTG_HS_DOEPCTL4 (  )  \ OTG device endpoint-4 control register
    OTG_HS_DEVICE $3A0 + constant OTG_HS_DEVICE_OTG_HS_DOEPCTL5 (  )  \ OTG device endpoint-5 control register
    OTG_HS_DEVICE $3C0 + constant OTG_HS_DEVICE_OTG_HS_DOEPCTL6 (  )  \ OTG device endpoint-6 control register
    OTG_HS_DEVICE $3E0 + constant OTG_HS_DEVICE_OTG_HS_DOEPCTL7 (  )  \ OTG device endpoint-7 control register
    OTG_HS_DEVICE $3B0 + constant OTG_HS_DEVICE_OTG_HS_DOEPTSIZ5 ( read-write )  \ OTG_HS device endpoint-5 transfer size register
    OTG_HS_DEVICE $3D0 + constant OTG_HS_DEVICE_OTG_HS_DOEPTSIZ6 ( read-write )  \ OTG_HS device endpoint-6 transfer size register
    OTG_HS_DEVICE $3F0 + constant OTG_HS_DEVICE_OTG_HS_DOEPTSIZ7 ( read-write )  \ OTG_HS device endpoint-7 transfer size register
    : OTG_HS_DEVICE_OTG_HS_DCFG. cr ." OTG_HS_DEVICE_OTG_HS_DCFG.  RW   $" OTG_HS_DEVICE_OTG_HS_DCFG @ hex. OTG_HS_DEVICE_OTG_HS_DCFG 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DCTL. cr ." OTG_HS_DEVICE_OTG_HS_DCTL.   $" OTG_HS_DEVICE_OTG_HS_DCTL @ hex. OTG_HS_DEVICE_OTG_HS_DCTL 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DSTS. cr ." OTG_HS_DEVICE_OTG_HS_DSTS.  RO   $" OTG_HS_DEVICE_OTG_HS_DSTS @ hex. OTG_HS_DEVICE_OTG_HS_DSTS 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DIEPMSK. cr ." OTG_HS_DEVICE_OTG_HS_DIEPMSK.  RW   $" OTG_HS_DEVICE_OTG_HS_DIEPMSK @ hex. OTG_HS_DEVICE_OTG_HS_DIEPMSK 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DOEPMSK. cr ." OTG_HS_DEVICE_OTG_HS_DOEPMSK.  RW   $" OTG_HS_DEVICE_OTG_HS_DOEPMSK @ hex. OTG_HS_DEVICE_OTG_HS_DOEPMSK 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DAINT. cr ." OTG_HS_DEVICE_OTG_HS_DAINT.  RO   $" OTG_HS_DEVICE_OTG_HS_DAINT @ hex. OTG_HS_DEVICE_OTG_HS_DAINT 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DAINTMSK. cr ." OTG_HS_DEVICE_OTG_HS_DAINTMSK.  RW   $" OTG_HS_DEVICE_OTG_HS_DAINTMSK @ hex. OTG_HS_DEVICE_OTG_HS_DAINTMSK 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DVBUSDIS. cr ." OTG_HS_DEVICE_OTG_HS_DVBUSDIS.  RW   $" OTG_HS_DEVICE_OTG_HS_DVBUSDIS @ hex. OTG_HS_DEVICE_OTG_HS_DVBUSDIS 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DVBUSPULSE. cr ." OTG_HS_DEVICE_OTG_HS_DVBUSPULSE.  RW   $" OTG_HS_DEVICE_OTG_HS_DVBUSPULSE @ hex. OTG_HS_DEVICE_OTG_HS_DVBUSPULSE 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DTHRCTL. cr ." OTG_HS_DEVICE_OTG_HS_DTHRCTL.  RW   $" OTG_HS_DEVICE_OTG_HS_DTHRCTL @ hex. OTG_HS_DEVICE_OTG_HS_DTHRCTL 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DIEPEMPMSK. cr ." OTG_HS_DEVICE_OTG_HS_DIEPEMPMSK.  RW   $" OTG_HS_DEVICE_OTG_HS_DIEPEMPMSK @ hex. OTG_HS_DEVICE_OTG_HS_DIEPEMPMSK 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DEACHINT. cr ." OTG_HS_DEVICE_OTG_HS_DEACHINT.  RW   $" OTG_HS_DEVICE_OTG_HS_DEACHINT @ hex. OTG_HS_DEVICE_OTG_HS_DEACHINT 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DEACHINTMSK. cr ." OTG_HS_DEVICE_OTG_HS_DEACHINTMSK.  RW   $" OTG_HS_DEVICE_OTG_HS_DEACHINTMSK @ hex. OTG_HS_DEVICE_OTG_HS_DEACHINTMSK 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DIEPCTL0. cr ." OTG_HS_DEVICE_OTG_HS_DIEPCTL0.   $" OTG_HS_DEVICE_OTG_HS_DIEPCTL0 @ hex. OTG_HS_DEVICE_OTG_HS_DIEPCTL0 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DIEPCTL1. cr ." OTG_HS_DEVICE_OTG_HS_DIEPCTL1.   $" OTG_HS_DEVICE_OTG_HS_DIEPCTL1 @ hex. OTG_HS_DEVICE_OTG_HS_DIEPCTL1 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DIEPCTL2. cr ." OTG_HS_DEVICE_OTG_HS_DIEPCTL2.   $" OTG_HS_DEVICE_OTG_HS_DIEPCTL2 @ hex. OTG_HS_DEVICE_OTG_HS_DIEPCTL2 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DIEPCTL3. cr ." OTG_HS_DEVICE_OTG_HS_DIEPCTL3.   $" OTG_HS_DEVICE_OTG_HS_DIEPCTL3 @ hex. OTG_HS_DEVICE_OTG_HS_DIEPCTL3 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DIEPCTL4. cr ." OTG_HS_DEVICE_OTG_HS_DIEPCTL4.   $" OTG_HS_DEVICE_OTG_HS_DIEPCTL4 @ hex. OTG_HS_DEVICE_OTG_HS_DIEPCTL4 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DIEPCTL5. cr ." OTG_HS_DEVICE_OTG_HS_DIEPCTL5.   $" OTG_HS_DEVICE_OTG_HS_DIEPCTL5 @ hex. OTG_HS_DEVICE_OTG_HS_DIEPCTL5 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DIEPCTL6. cr ." OTG_HS_DEVICE_OTG_HS_DIEPCTL6.   $" OTG_HS_DEVICE_OTG_HS_DIEPCTL6 @ hex. OTG_HS_DEVICE_OTG_HS_DIEPCTL6 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DIEPCTL7. cr ." OTG_HS_DEVICE_OTG_HS_DIEPCTL7.   $" OTG_HS_DEVICE_OTG_HS_DIEPCTL7 @ hex. OTG_HS_DEVICE_OTG_HS_DIEPCTL7 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DIEPINT0. cr ." OTG_HS_DEVICE_OTG_HS_DIEPINT0.   $" OTG_HS_DEVICE_OTG_HS_DIEPINT0 @ hex. OTG_HS_DEVICE_OTG_HS_DIEPINT0 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DIEPINT1. cr ." OTG_HS_DEVICE_OTG_HS_DIEPINT1.   $" OTG_HS_DEVICE_OTG_HS_DIEPINT1 @ hex. OTG_HS_DEVICE_OTG_HS_DIEPINT1 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DIEPINT2. cr ." OTG_HS_DEVICE_OTG_HS_DIEPINT2.   $" OTG_HS_DEVICE_OTG_HS_DIEPINT2 @ hex. OTG_HS_DEVICE_OTG_HS_DIEPINT2 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DIEPINT3. cr ." OTG_HS_DEVICE_OTG_HS_DIEPINT3.   $" OTG_HS_DEVICE_OTG_HS_DIEPINT3 @ hex. OTG_HS_DEVICE_OTG_HS_DIEPINT3 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DIEPINT4. cr ." OTG_HS_DEVICE_OTG_HS_DIEPINT4.   $" OTG_HS_DEVICE_OTG_HS_DIEPINT4 @ hex. OTG_HS_DEVICE_OTG_HS_DIEPINT4 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DIEPINT5. cr ." OTG_HS_DEVICE_OTG_HS_DIEPINT5.   $" OTG_HS_DEVICE_OTG_HS_DIEPINT5 @ hex. OTG_HS_DEVICE_OTG_HS_DIEPINT5 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DIEPINT6. cr ." OTG_HS_DEVICE_OTG_HS_DIEPINT6.   $" OTG_HS_DEVICE_OTG_HS_DIEPINT6 @ hex. OTG_HS_DEVICE_OTG_HS_DIEPINT6 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DIEPINT7. cr ." OTG_HS_DEVICE_OTG_HS_DIEPINT7.   $" OTG_HS_DEVICE_OTG_HS_DIEPINT7 @ hex. OTG_HS_DEVICE_OTG_HS_DIEPINT7 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DIEPTSIZ0. cr ." OTG_HS_DEVICE_OTG_HS_DIEPTSIZ0.  RW   $" OTG_HS_DEVICE_OTG_HS_DIEPTSIZ0 @ hex. OTG_HS_DEVICE_OTG_HS_DIEPTSIZ0 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DIEPDMA1. cr ." OTG_HS_DEVICE_OTG_HS_DIEPDMA1.  RW   $" OTG_HS_DEVICE_OTG_HS_DIEPDMA1 @ hex. OTG_HS_DEVICE_OTG_HS_DIEPDMA1 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DIEPDMA2. cr ." OTG_HS_DEVICE_OTG_HS_DIEPDMA2.  RW   $" OTG_HS_DEVICE_OTG_HS_DIEPDMA2 @ hex. OTG_HS_DEVICE_OTG_HS_DIEPDMA2 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DIEPDMA3. cr ." OTG_HS_DEVICE_OTG_HS_DIEPDMA3.  RW   $" OTG_HS_DEVICE_OTG_HS_DIEPDMA3 @ hex. OTG_HS_DEVICE_OTG_HS_DIEPDMA3 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DIEPDMA4. cr ." OTG_HS_DEVICE_OTG_HS_DIEPDMA4.  RW   $" OTG_HS_DEVICE_OTG_HS_DIEPDMA4 @ hex. OTG_HS_DEVICE_OTG_HS_DIEPDMA4 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DIEPDMA5. cr ." OTG_HS_DEVICE_OTG_HS_DIEPDMA5.  RW   $" OTG_HS_DEVICE_OTG_HS_DIEPDMA5 @ hex. OTG_HS_DEVICE_OTG_HS_DIEPDMA5 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DTXFSTS0. cr ." OTG_HS_DEVICE_OTG_HS_DTXFSTS0.  RO   $" OTG_HS_DEVICE_OTG_HS_DTXFSTS0 @ hex. OTG_HS_DEVICE_OTG_HS_DTXFSTS0 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DTXFSTS1. cr ." OTG_HS_DEVICE_OTG_HS_DTXFSTS1.  RO   $" OTG_HS_DEVICE_OTG_HS_DTXFSTS1 @ hex. OTG_HS_DEVICE_OTG_HS_DTXFSTS1 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DTXFSTS2. cr ." OTG_HS_DEVICE_OTG_HS_DTXFSTS2.  RO   $" OTG_HS_DEVICE_OTG_HS_DTXFSTS2 @ hex. OTG_HS_DEVICE_OTG_HS_DTXFSTS2 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DTXFSTS3. cr ." OTG_HS_DEVICE_OTG_HS_DTXFSTS3.  RO   $" OTG_HS_DEVICE_OTG_HS_DTXFSTS3 @ hex. OTG_HS_DEVICE_OTG_HS_DTXFSTS3 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DTXFSTS4. cr ." OTG_HS_DEVICE_OTG_HS_DTXFSTS4.  RO   $" OTG_HS_DEVICE_OTG_HS_DTXFSTS4 @ hex. OTG_HS_DEVICE_OTG_HS_DTXFSTS4 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DTXFSTS5. cr ." OTG_HS_DEVICE_OTG_HS_DTXFSTS5.  RO   $" OTG_HS_DEVICE_OTG_HS_DTXFSTS5 @ hex. OTG_HS_DEVICE_OTG_HS_DTXFSTS5 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DIEPTSIZ1. cr ." OTG_HS_DEVICE_OTG_HS_DIEPTSIZ1.  RW   $" OTG_HS_DEVICE_OTG_HS_DIEPTSIZ1 @ hex. OTG_HS_DEVICE_OTG_HS_DIEPTSIZ1 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DIEPTSIZ2. cr ." OTG_HS_DEVICE_OTG_HS_DIEPTSIZ2.  RW   $" OTG_HS_DEVICE_OTG_HS_DIEPTSIZ2 @ hex. OTG_HS_DEVICE_OTG_HS_DIEPTSIZ2 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DIEPTSIZ3. cr ." OTG_HS_DEVICE_OTG_HS_DIEPTSIZ3.  RW   $" OTG_HS_DEVICE_OTG_HS_DIEPTSIZ3 @ hex. OTG_HS_DEVICE_OTG_HS_DIEPTSIZ3 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DIEPTSIZ4. cr ." OTG_HS_DEVICE_OTG_HS_DIEPTSIZ4.  RW   $" OTG_HS_DEVICE_OTG_HS_DIEPTSIZ4 @ hex. OTG_HS_DEVICE_OTG_HS_DIEPTSIZ4 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DIEPTSIZ5. cr ." OTG_HS_DEVICE_OTG_HS_DIEPTSIZ5.  RW   $" OTG_HS_DEVICE_OTG_HS_DIEPTSIZ5 @ hex. OTG_HS_DEVICE_OTG_HS_DIEPTSIZ5 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DOEPCTL0. cr ." OTG_HS_DEVICE_OTG_HS_DOEPCTL0.   $" OTG_HS_DEVICE_OTG_HS_DOEPCTL0 @ hex. OTG_HS_DEVICE_OTG_HS_DOEPCTL0 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DOEPCTL1. cr ." OTG_HS_DEVICE_OTG_HS_DOEPCTL1.   $" OTG_HS_DEVICE_OTG_HS_DOEPCTL1 @ hex. OTG_HS_DEVICE_OTG_HS_DOEPCTL1 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DOEPCTL2. cr ." OTG_HS_DEVICE_OTG_HS_DOEPCTL2.   $" OTG_HS_DEVICE_OTG_HS_DOEPCTL2 @ hex. OTG_HS_DEVICE_OTG_HS_DOEPCTL2 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DOEPCTL3. cr ." OTG_HS_DEVICE_OTG_HS_DOEPCTL3.   $" OTG_HS_DEVICE_OTG_HS_DOEPCTL3 @ hex. OTG_HS_DEVICE_OTG_HS_DOEPCTL3 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DOEPINT0. cr ." OTG_HS_DEVICE_OTG_HS_DOEPINT0.  RW   $" OTG_HS_DEVICE_OTG_HS_DOEPINT0 @ hex. OTG_HS_DEVICE_OTG_HS_DOEPINT0 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DOEPINT1. cr ." OTG_HS_DEVICE_OTG_HS_DOEPINT1.  RW   $" OTG_HS_DEVICE_OTG_HS_DOEPINT1 @ hex. OTG_HS_DEVICE_OTG_HS_DOEPINT1 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DOEPINT2. cr ." OTG_HS_DEVICE_OTG_HS_DOEPINT2.  RW   $" OTG_HS_DEVICE_OTG_HS_DOEPINT2 @ hex. OTG_HS_DEVICE_OTG_HS_DOEPINT2 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DOEPINT3. cr ." OTG_HS_DEVICE_OTG_HS_DOEPINT3.  RW   $" OTG_HS_DEVICE_OTG_HS_DOEPINT3 @ hex. OTG_HS_DEVICE_OTG_HS_DOEPINT3 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DOEPINT4. cr ." OTG_HS_DEVICE_OTG_HS_DOEPINT4.  RW   $" OTG_HS_DEVICE_OTG_HS_DOEPINT4 @ hex. OTG_HS_DEVICE_OTG_HS_DOEPINT4 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DOEPINT5. cr ." OTG_HS_DEVICE_OTG_HS_DOEPINT5.  RW   $" OTG_HS_DEVICE_OTG_HS_DOEPINT5 @ hex. OTG_HS_DEVICE_OTG_HS_DOEPINT5 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DOEPINT6. cr ." OTG_HS_DEVICE_OTG_HS_DOEPINT6.  RW   $" OTG_HS_DEVICE_OTG_HS_DOEPINT6 @ hex. OTG_HS_DEVICE_OTG_HS_DOEPINT6 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DOEPINT7. cr ." OTG_HS_DEVICE_OTG_HS_DOEPINT7.  RW   $" OTG_HS_DEVICE_OTG_HS_DOEPINT7 @ hex. OTG_HS_DEVICE_OTG_HS_DOEPINT7 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DOEPTSIZ0. cr ." OTG_HS_DEVICE_OTG_HS_DOEPTSIZ0.  RW   $" OTG_HS_DEVICE_OTG_HS_DOEPTSIZ0 @ hex. OTG_HS_DEVICE_OTG_HS_DOEPTSIZ0 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DOEPTSIZ1. cr ." OTG_HS_DEVICE_OTG_HS_DOEPTSIZ1.  RW   $" OTG_HS_DEVICE_OTG_HS_DOEPTSIZ1 @ hex. OTG_HS_DEVICE_OTG_HS_DOEPTSIZ1 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DOEPTSIZ2. cr ." OTG_HS_DEVICE_OTG_HS_DOEPTSIZ2.  RW   $" OTG_HS_DEVICE_OTG_HS_DOEPTSIZ2 @ hex. OTG_HS_DEVICE_OTG_HS_DOEPTSIZ2 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DOEPTSIZ3. cr ." OTG_HS_DEVICE_OTG_HS_DOEPTSIZ3.  RW   $" OTG_HS_DEVICE_OTG_HS_DOEPTSIZ3 @ hex. OTG_HS_DEVICE_OTG_HS_DOEPTSIZ3 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DOEPTSIZ4. cr ." OTG_HS_DEVICE_OTG_HS_DOEPTSIZ4.  RW   $" OTG_HS_DEVICE_OTG_HS_DOEPTSIZ4 @ hex. OTG_HS_DEVICE_OTG_HS_DOEPTSIZ4 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DIEPTSIZ6. cr ." OTG_HS_DEVICE_OTG_HS_DIEPTSIZ6.  RW   $" OTG_HS_DEVICE_OTG_HS_DIEPTSIZ6 @ hex. OTG_HS_DEVICE_OTG_HS_DIEPTSIZ6 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DTXFSTS6. cr ." OTG_HS_DEVICE_OTG_HS_DTXFSTS6.  RW   $" OTG_HS_DEVICE_OTG_HS_DTXFSTS6 @ hex. OTG_HS_DEVICE_OTG_HS_DTXFSTS6 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DIEPTSIZ7. cr ." OTG_HS_DEVICE_OTG_HS_DIEPTSIZ7.  RW   $" OTG_HS_DEVICE_OTG_HS_DIEPTSIZ7 @ hex. OTG_HS_DEVICE_OTG_HS_DIEPTSIZ7 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DTXFSTS7. cr ." OTG_HS_DEVICE_OTG_HS_DTXFSTS7.  RW   $" OTG_HS_DEVICE_OTG_HS_DTXFSTS7 @ hex. OTG_HS_DEVICE_OTG_HS_DTXFSTS7 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DOEPCTL4. cr ." OTG_HS_DEVICE_OTG_HS_DOEPCTL4.   $" OTG_HS_DEVICE_OTG_HS_DOEPCTL4 @ hex. OTG_HS_DEVICE_OTG_HS_DOEPCTL4 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DOEPCTL5. cr ." OTG_HS_DEVICE_OTG_HS_DOEPCTL5.   $" OTG_HS_DEVICE_OTG_HS_DOEPCTL5 @ hex. OTG_HS_DEVICE_OTG_HS_DOEPCTL5 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DOEPCTL6. cr ." OTG_HS_DEVICE_OTG_HS_DOEPCTL6.   $" OTG_HS_DEVICE_OTG_HS_DOEPCTL6 @ hex. OTG_HS_DEVICE_OTG_HS_DOEPCTL6 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DOEPCTL7. cr ." OTG_HS_DEVICE_OTG_HS_DOEPCTL7.   $" OTG_HS_DEVICE_OTG_HS_DOEPCTL7 @ hex. OTG_HS_DEVICE_OTG_HS_DOEPCTL7 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DOEPTSIZ5. cr ." OTG_HS_DEVICE_OTG_HS_DOEPTSIZ5.  RW   $" OTG_HS_DEVICE_OTG_HS_DOEPTSIZ5 @ hex. OTG_HS_DEVICE_OTG_HS_DOEPTSIZ5 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DOEPTSIZ6. cr ." OTG_HS_DEVICE_OTG_HS_DOEPTSIZ6.  RW   $" OTG_HS_DEVICE_OTG_HS_DOEPTSIZ6 @ hex. OTG_HS_DEVICE_OTG_HS_DOEPTSIZ6 1b. ;
    : OTG_HS_DEVICE_OTG_HS_DOEPTSIZ7. cr ." OTG_HS_DEVICE_OTG_HS_DOEPTSIZ7.  RW   $" OTG_HS_DEVICE_OTG_HS_DOEPTSIZ7 @ hex. OTG_HS_DEVICE_OTG_HS_DOEPTSIZ7 1b. ;
    : OTG_HS_DEVICE.
      OTG_HS_DEVICE_OTG_HS_DCFG.
      OTG_HS_DEVICE_OTG_HS_DCTL.
      OTG_HS_DEVICE_OTG_HS_DSTS.
      OTG_HS_DEVICE_OTG_HS_DIEPMSK.
      OTG_HS_DEVICE_OTG_HS_DOEPMSK.
      OTG_HS_DEVICE_OTG_HS_DAINT.
      OTG_HS_DEVICE_OTG_HS_DAINTMSK.
      OTG_HS_DEVICE_OTG_HS_DVBUSDIS.
      OTG_HS_DEVICE_OTG_HS_DVBUSPULSE.
      OTG_HS_DEVICE_OTG_HS_DTHRCTL.
      OTG_HS_DEVICE_OTG_HS_DIEPEMPMSK.
      OTG_HS_DEVICE_OTG_HS_DEACHINT.
      OTG_HS_DEVICE_OTG_HS_DEACHINTMSK.
      OTG_HS_DEVICE_OTG_HS_DIEPCTL0.
      OTG_HS_DEVICE_OTG_HS_DIEPCTL1.
      OTG_HS_DEVICE_OTG_HS_DIEPCTL2.
      OTG_HS_DEVICE_OTG_HS_DIEPCTL3.
      OTG_HS_DEVICE_OTG_HS_DIEPCTL4.
      OTG_HS_DEVICE_OTG_HS_DIEPCTL5.
      OTG_HS_DEVICE_OTG_HS_DIEPCTL6.
      OTG_HS_DEVICE_OTG_HS_DIEPCTL7.
      OTG_HS_DEVICE_OTG_HS_DIEPINT0.
      OTG_HS_DEVICE_OTG_HS_DIEPINT1.
      OTG_HS_DEVICE_OTG_HS_DIEPINT2.
      OTG_HS_DEVICE_OTG_HS_DIEPINT3.
      OTG_HS_DEVICE_OTG_HS_DIEPINT4.
      OTG_HS_DEVICE_OTG_HS_DIEPINT5.
      OTG_HS_DEVICE_OTG_HS_DIEPINT6.
      OTG_HS_DEVICE_OTG_HS_DIEPINT7.
      OTG_HS_DEVICE_OTG_HS_DIEPTSIZ0.
      OTG_HS_DEVICE_OTG_HS_DIEPDMA1.
      OTG_HS_DEVICE_OTG_HS_DIEPDMA2.
      OTG_HS_DEVICE_OTG_HS_DIEPDMA3.
      OTG_HS_DEVICE_OTG_HS_DIEPDMA4.
      OTG_HS_DEVICE_OTG_HS_DIEPDMA5.
      OTG_HS_DEVICE_OTG_HS_DTXFSTS0.
      OTG_HS_DEVICE_OTG_HS_DTXFSTS1.
      OTG_HS_DEVICE_OTG_HS_DTXFSTS2.
      OTG_HS_DEVICE_OTG_HS_DTXFSTS3.
      OTG_HS_DEVICE_OTG_HS_DTXFSTS4.
      OTG_HS_DEVICE_OTG_HS_DTXFSTS5.
      OTG_HS_DEVICE_OTG_HS_DIEPTSIZ1.
      OTG_HS_DEVICE_OTG_HS_DIEPTSIZ2.
      OTG_HS_DEVICE_OTG_HS_DIEPTSIZ3.
      OTG_HS_DEVICE_OTG_HS_DIEPTSIZ4.
      OTG_HS_DEVICE_OTG_HS_DIEPTSIZ5.
      OTG_HS_DEVICE_OTG_HS_DOEPCTL0.
      OTG_HS_DEVICE_OTG_HS_DOEPCTL1.
      OTG_HS_DEVICE_OTG_HS_DOEPCTL2.
      OTG_HS_DEVICE_OTG_HS_DOEPCTL3.
      OTG_HS_DEVICE_OTG_HS_DOEPINT0.
      OTG_HS_DEVICE_OTG_HS_DOEPINT1.
      OTG_HS_DEVICE_OTG_HS_DOEPINT2.
      OTG_HS_DEVICE_OTG_HS_DOEPINT3.
      OTG_HS_DEVICE_OTG_HS_DOEPINT4.
      OTG_HS_DEVICE_OTG_HS_DOEPINT5.
      OTG_HS_DEVICE_OTG_HS_DOEPINT6.
      OTG_HS_DEVICE_OTG_HS_DOEPINT7.
      OTG_HS_DEVICE_OTG_HS_DOEPTSIZ0.
      OTG_HS_DEVICE_OTG_HS_DOEPTSIZ1.
      OTG_HS_DEVICE_OTG_HS_DOEPTSIZ2.
      OTG_HS_DEVICE_OTG_HS_DOEPTSIZ3.
      OTG_HS_DEVICE_OTG_HS_DOEPTSIZ4.
      OTG_HS_DEVICE_OTG_HS_DIEPTSIZ6.
      OTG_HS_DEVICE_OTG_HS_DTXFSTS6.
      OTG_HS_DEVICE_OTG_HS_DIEPTSIZ7.
      OTG_HS_DEVICE_OTG_HS_DTXFSTS7.
      OTG_HS_DEVICE_OTG_HS_DOEPCTL4.
      OTG_HS_DEVICE_OTG_HS_DOEPCTL5.
      OTG_HS_DEVICE_OTG_HS_DOEPCTL6.
      OTG_HS_DEVICE_OTG_HS_DOEPCTL7.
      OTG_HS_DEVICE_OTG_HS_DOEPTSIZ5.
      OTG_HS_DEVICE_OTG_HS_DOEPTSIZ6.
      OTG_HS_DEVICE_OTG_HS_DOEPTSIZ7.
    ;
  [then]

  execute-defined? use-OTG_HS_PWRCLK [if]
    $40040E00 constant OTG_HS_PWRCLK ( USB on the go high speed ) 
    OTG_HS_PWRCLK $0 + constant OTG_HS_PWRCLK_OTG_HS_PCGCR ( read-write )  \ Power and clock gating control register
    : OTG_HS_PWRCLK_OTG_HS_PCGCR. cr ." OTG_HS_PWRCLK_OTG_HS_PCGCR.  RW   $" OTG_HS_PWRCLK_OTG_HS_PCGCR @ hex. OTG_HS_PWRCLK_OTG_HS_PCGCR 1b. ;
    : OTG_HS_PWRCLK.
      OTG_HS_PWRCLK_OTG_HS_PCGCR.
    ;
  [then]

  execute-defined? use-NVIC [if]
    $E000E100 constant NVIC ( Nested Vectored Interrupt Controller ) 
    NVIC $0 + constant NVIC_ISER0 ( read-write )  \ Interrupt Set-Enable Register
    NVIC $4 + constant NVIC_ISER1 ( read-write )  \ Interrupt Set-Enable Register
    NVIC $8 + constant NVIC_ISER2 ( read-write )  \ Interrupt Set-Enable Register
    NVIC $80 + constant NVIC_ICER0 ( read-write )  \ Interrupt Clear-Enable Register
    NVIC $84 + constant NVIC_ICER1 ( read-write )  \ Interrupt Clear-Enable Register
    NVIC $88 + constant NVIC_ICER2 ( read-write )  \ Interrupt Clear-Enable Register
    NVIC $100 + constant NVIC_ISPR0 ( read-write )  \ Interrupt Set-Pending Register
    NVIC $104 + constant NVIC_ISPR1 ( read-write )  \ Interrupt Set-Pending Register
    NVIC $108 + constant NVIC_ISPR2 ( read-write )  \ Interrupt Set-Pending Register
    NVIC $180 + constant NVIC_ICPR0 ( read-write )  \ Interrupt Clear-Pending Register
    NVIC $184 + constant NVIC_ICPR1 ( read-write )  \ Interrupt Clear-Pending Register
    NVIC $188 + constant NVIC_ICPR2 ( read-write )  \ Interrupt Clear-Pending Register
    NVIC $200 + constant NVIC_IABR0 ( read-only )  \ Interrupt Active Bit Register
    NVIC $204 + constant NVIC_IABR1 ( read-only )  \ Interrupt Active Bit Register
    NVIC $208 + constant NVIC_IABR2 ( read-only )  \ Interrupt Active Bit Register
    NVIC $300 + constant NVIC_IPR0 ( read-write )  \ Interrupt Priority Register
    NVIC $304 + constant NVIC_IPR1 ( read-write )  \ Interrupt Priority Register
    NVIC $308 + constant NVIC_IPR2 ( read-write )  \ Interrupt Priority Register
    NVIC $30C + constant NVIC_IPR3 ( read-write )  \ Interrupt Priority Register
    NVIC $310 + constant NVIC_IPR4 ( read-write )  \ Interrupt Priority Register
    NVIC $314 + constant NVIC_IPR5 ( read-write )  \ Interrupt Priority Register
    NVIC $318 + constant NVIC_IPR6 ( read-write )  \ Interrupt Priority Register
    NVIC $31C + constant NVIC_IPR7 ( read-write )  \ Interrupt Priority Register
    NVIC $320 + constant NVIC_IPR8 ( read-write )  \ Interrupt Priority Register
    NVIC $324 + constant NVIC_IPR9 ( read-write )  \ Interrupt Priority Register
    NVIC $328 + constant NVIC_IPR10 ( read-write )  \ Interrupt Priority Register
    NVIC $32C + constant NVIC_IPR11 ( read-write )  \ Interrupt Priority Register
    NVIC $330 + constant NVIC_IPR12 ( read-write )  \ Interrupt Priority Register
    NVIC $334 + constant NVIC_IPR13 ( read-write )  \ Interrupt Priority Register
    NVIC $338 + constant NVIC_IPR14 ( read-write )  \ Interrupt Priority Register
    NVIC $33C + constant NVIC_IPR15 ( read-write )  \ Interrupt Priority Register
    NVIC $340 + constant NVIC_IPR16 ( read-write )  \ Interrupt Priority Register
    NVIC $344 + constant NVIC_IPR17 ( read-write )  \ Interrupt Priority Register
    NVIC $348 + constant NVIC_IPR18 ( read-write )  \ Interrupt Priority Register
    NVIC $34C + constant NVIC_IPR19 ( read-write )  \ Interrupt Priority Register
    NVIC $350 + constant NVIC_IPR20 ( read-write )  \ Interrupt Priority Register
    NVIC $354 + constant NVIC_IPR21 ( read-write )  \ Interrupt Priority Register
    NVIC $358 + constant NVIC_IPR22 ( read-write )  \ Interrupt Priority Register
    NVIC $35C + constant NVIC_IPR23 ( read-write )  \ Interrupt Priority Register
    NVIC $360 + constant NVIC_IPR24 ( read-write )  \ Interrupt Priority Register
    NVIC $364 + constant NVIC_IPR25 ( read-write )  \ Interrupt Priority Register
    : NVIC_ISER0. cr ." NVIC_ISER0.  RW   $" NVIC_ISER0 @ hex. NVIC_ISER0 1b. ;
    : NVIC_ISER1. cr ." NVIC_ISER1.  RW   $" NVIC_ISER1 @ hex. NVIC_ISER1 1b. ;
    : NVIC_ISER2. cr ." NVIC_ISER2.  RW   $" NVIC_ISER2 @ hex. NVIC_ISER2 1b. ;
    : NVIC_ICER0. cr ." NVIC_ICER0.  RW   $" NVIC_ICER0 @ hex. NVIC_ICER0 1b. ;
    : NVIC_ICER1. cr ." NVIC_ICER1.  RW   $" NVIC_ICER1 @ hex. NVIC_ICER1 1b. ;
    : NVIC_ICER2. cr ." NVIC_ICER2.  RW   $" NVIC_ICER2 @ hex. NVIC_ICER2 1b. ;
    : NVIC_ISPR0. cr ." NVIC_ISPR0.  RW   $" NVIC_ISPR0 @ hex. NVIC_ISPR0 1b. ;
    : NVIC_ISPR1. cr ." NVIC_ISPR1.  RW   $" NVIC_ISPR1 @ hex. NVIC_ISPR1 1b. ;
    : NVIC_ISPR2. cr ." NVIC_ISPR2.  RW   $" NVIC_ISPR2 @ hex. NVIC_ISPR2 1b. ;
    : NVIC_ICPR0. cr ." NVIC_ICPR0.  RW   $" NVIC_ICPR0 @ hex. NVIC_ICPR0 1b. ;
    : NVIC_ICPR1. cr ." NVIC_ICPR1.  RW   $" NVIC_ICPR1 @ hex. NVIC_ICPR1 1b. ;
    : NVIC_ICPR2. cr ." NVIC_ICPR2.  RW   $" NVIC_ICPR2 @ hex. NVIC_ICPR2 1b. ;
    : NVIC_IABR0. cr ." NVIC_IABR0.  RO   $" NVIC_IABR0 @ hex. NVIC_IABR0 1b. ;
    : NVIC_IABR1. cr ." NVIC_IABR1.  RO   $" NVIC_IABR1 @ hex. NVIC_IABR1 1b. ;
    : NVIC_IABR2. cr ." NVIC_IABR2.  RO   $" NVIC_IABR2 @ hex. NVIC_IABR2 1b. ;
    : NVIC_IPR0. cr ." NVIC_IPR0.  RW   $" NVIC_IPR0 @ hex. NVIC_IPR0 1b. ;
    : NVIC_IPR1. cr ." NVIC_IPR1.  RW   $" NVIC_IPR1 @ hex. NVIC_IPR1 1b. ;
    : NVIC_IPR2. cr ." NVIC_IPR2.  RW   $" NVIC_IPR2 @ hex. NVIC_IPR2 1b. ;
    : NVIC_IPR3. cr ." NVIC_IPR3.  RW   $" NVIC_IPR3 @ hex. NVIC_IPR3 1b. ;
    : NVIC_IPR4. cr ." NVIC_IPR4.  RW   $" NVIC_IPR4 @ hex. NVIC_IPR4 1b. ;
    : NVIC_IPR5. cr ." NVIC_IPR5.  RW   $" NVIC_IPR5 @ hex. NVIC_IPR5 1b. ;
    : NVIC_IPR6. cr ." NVIC_IPR6.  RW   $" NVIC_IPR6 @ hex. NVIC_IPR6 1b. ;
    : NVIC_IPR7. cr ." NVIC_IPR7.  RW   $" NVIC_IPR7 @ hex. NVIC_IPR7 1b. ;
    : NVIC_IPR8. cr ." NVIC_IPR8.  RW   $" NVIC_IPR8 @ hex. NVIC_IPR8 1b. ;
    : NVIC_IPR9. cr ." NVIC_IPR9.  RW   $" NVIC_IPR9 @ hex. NVIC_IPR9 1b. ;
    : NVIC_IPR10. cr ." NVIC_IPR10.  RW   $" NVIC_IPR10 @ hex. NVIC_IPR10 1b. ;
    : NVIC_IPR11. cr ." NVIC_IPR11.  RW   $" NVIC_IPR11 @ hex. NVIC_IPR11 1b. ;
    : NVIC_IPR12. cr ." NVIC_IPR12.  RW   $" NVIC_IPR12 @ hex. NVIC_IPR12 1b. ;
    : NVIC_IPR13. cr ." NVIC_IPR13.  RW   $" NVIC_IPR13 @ hex. NVIC_IPR13 1b. ;
    : NVIC_IPR14. cr ." NVIC_IPR14.  RW   $" NVIC_IPR14 @ hex. NVIC_IPR14 1b. ;
    : NVIC_IPR15. cr ." NVIC_IPR15.  RW   $" NVIC_IPR15 @ hex. NVIC_IPR15 1b. ;
    : NVIC_IPR16. cr ." NVIC_IPR16.  RW   $" NVIC_IPR16 @ hex. NVIC_IPR16 1b. ;
    : NVIC_IPR17. cr ." NVIC_IPR17.  RW   $" NVIC_IPR17 @ hex. NVIC_IPR17 1b. ;
    : NVIC_IPR18. cr ." NVIC_IPR18.  RW   $" NVIC_IPR18 @ hex. NVIC_IPR18 1b. ;
    : NVIC_IPR19. cr ." NVIC_IPR19.  RW   $" NVIC_IPR19 @ hex. NVIC_IPR19 1b. ;
    : NVIC_IPR20. cr ." NVIC_IPR20.  RW   $" NVIC_IPR20 @ hex. NVIC_IPR20 1b. ;
    : NVIC_IPR21. cr ." NVIC_IPR21.  RW   $" NVIC_IPR21 @ hex. NVIC_IPR21 1b. ;
    : NVIC_IPR22. cr ." NVIC_IPR22.  RW   $" NVIC_IPR22 @ hex. NVIC_IPR22 1b. ;
    : NVIC_IPR23. cr ." NVIC_IPR23.  RW   $" NVIC_IPR23 @ hex. NVIC_IPR23 1b. ;
    : NVIC_IPR24. cr ." NVIC_IPR24.  RW   $" NVIC_IPR24 @ hex. NVIC_IPR24 1b. ;
    : NVIC_IPR25. cr ." NVIC_IPR25.  RW   $" NVIC_IPR25 @ hex. NVIC_IPR25 1b. ;
    : NVIC.
      NVIC_ISER0.
      NVIC_ISER1.
      NVIC_ISER2.
      NVIC_ICER0.
      NVIC_ICER1.
      NVIC_ICER2.
      NVIC_ISPR0.
      NVIC_ISPR1.
      NVIC_ISPR2.
      NVIC_ICPR0.
      NVIC_ICPR1.
      NVIC_ICPR2.
      NVIC_IABR0.
      NVIC_IABR1.
      NVIC_IABR2.
      NVIC_IPR0.
      NVIC_IPR1.
      NVIC_IPR2.
      NVIC_IPR3.
      NVIC_IPR4.
      NVIC_IPR5.
      NVIC_IPR6.
      NVIC_IPR7.
      NVIC_IPR8.
      NVIC_IPR9.
      NVIC_IPR10.
      NVIC_IPR11.
      NVIC_IPR12.
      NVIC_IPR13.
      NVIC_IPR14.
      NVIC_IPR15.
      NVIC_IPR16.
      NVIC_IPR17.
      NVIC_IPR18.
      NVIC_IPR19.
      NVIC_IPR20.
      NVIC_IPR21.
      NVIC_IPR22.
      NVIC_IPR23.
      NVIC_IPR24.
      NVIC_IPR25.
    ;
  [then]

  execute-defined? use-MPU [if]
    $E000ED90 constant MPU ( Memory protection unit ) 
    MPU $0 + constant MPU_MPU_TYPER ( read-only )  \ MPU type register
    MPU $4 + constant MPU_MPU_CTRL ( read-only )  \ MPU control register
    MPU $8 + constant MPU_MPU_RNR ( read-write )  \ MPU region number register
    MPU $C + constant MPU_MPU_RBAR ( read-write )  \ MPU region base address register
    MPU $10 + constant MPU_MPU_RASR ( read-write )  \ MPU region attribute and size register
    : MPU_MPU_TYPER. cr ." MPU_MPU_TYPER.  RO   $" MPU_MPU_TYPER @ hex. MPU_MPU_TYPER 1b. ;
    : MPU_MPU_CTRL. cr ." MPU_MPU_CTRL.  RO   $" MPU_MPU_CTRL @ hex. MPU_MPU_CTRL 1b. ;
    : MPU_MPU_RNR. cr ." MPU_MPU_RNR.  RW   $" MPU_MPU_RNR @ hex. MPU_MPU_RNR 1b. ;
    : MPU_MPU_RBAR. cr ." MPU_MPU_RBAR.  RW   $" MPU_MPU_RBAR @ hex. MPU_MPU_RBAR 1b. ;
    : MPU_MPU_RASR. cr ." MPU_MPU_RASR.  RW   $" MPU_MPU_RASR @ hex. MPU_MPU_RASR 1b. ;
    : MPU.
      MPU_MPU_TYPER.
      MPU_MPU_CTRL.
      MPU_MPU_RNR.
      MPU_MPU_RBAR.
      MPU_MPU_RASR.
    ;
  [then]

  execute-defined? use-STK [if]
    $E000E010 constant STK ( SysTick timer ) 
    STK $0 + constant STK_CSR ( read-write )  \ SysTick control and status register
    STK $4 + constant STK_RVR ( read-write )  \ SysTick reload value register
    STK $8 + constant STK_CVR ( read-write )  \ SysTick current value register
    STK $C + constant STK_CALIB ( read-write )  \ SysTick calibration value register
    : STK_CSR. cr ." STK_CSR.  RW   $" STK_CSR @ hex. STK_CSR 1b. ;
    : STK_RVR. cr ." STK_RVR.  RW   $" STK_RVR @ hex. STK_RVR 1b. ;
    : STK_CVR. cr ." STK_CVR.  RW   $" STK_CVR @ hex. STK_CVR 1b. ;
    : STK_CALIB. cr ." STK_CALIB.  RW   $" STK_CALIB @ hex. STK_CALIB 1b. ;
    : STK.
      STK_CSR.
      STK_RVR.
      STK_CVR.
      STK_CALIB.
    ;
  [then]

  execute-defined? use-NVIC_STIR [if]
    $E000EF00 constant NVIC_STIR ( Nested vectored interrupt controller ) 
    NVIC_STIR $0 + constant NVIC_STIR_STIR ( read-write )  \ Software trigger interrupt register
    : NVIC_STIR_STIR. cr ." NVIC_STIR_STIR.  RW   $" NVIC_STIR_STIR @ hex. NVIC_STIR_STIR 1b. ;
    : NVIC_STIR.
      NVIC_STIR_STIR.
    ;
  [then]

  execute-defined? use-FPU_CPACR [if]
    $E000ED88 constant FPU_CPACR ( Floating point unit CPACR ) 
    FPU_CPACR $0 + constant FPU_CPACR_CPACR ( read-write )  \ Coprocessor access control register
    : FPU_CPACR_CPACR. cr ." FPU_CPACR_CPACR.  RW   $" FPU_CPACR_CPACR @ hex. FPU_CPACR_CPACR 1b. ;
    : FPU_CPACR.
      FPU_CPACR_CPACR.
    ;
  [then]

  execute-defined? use-SCB_ACTRL [if]
    $E000E008 constant SCB_ACTRL ( System control block ACTLR ) 
    SCB_ACTRL $0 + constant SCB_ACTRL_ACTRL ( read-write )  \ Auxiliary control register
    : SCB_ACTRL_ACTRL. cr ." SCB_ACTRL_ACTRL.  RW   $" SCB_ACTRL_ACTRL @ hex. SCB_ACTRL_ACTRL 1b. ;
    : SCB_ACTRL.
      SCB_ACTRL_ACTRL.
    ;
  [then]

  execute-defined? use-FPU [if]
    $E000EF34 constant FPU ( Floting point unit ) 
    FPU $0 + constant FPU_FPCCR ( read-write )  \ Floating-point context control register
    FPU $4 + constant FPU_FPCAR ( read-write )  \ Floating-point context address register
    FPU $8 + constant FPU_FPSCR ( read-write )  \ Floating-point status control register
    : FPU_FPCCR. cr ." FPU_FPCCR.  RW   $" FPU_FPCCR @ hex. FPU_FPCCR 1b. ;
    : FPU_FPCAR. cr ." FPU_FPCAR.  RW   $" FPU_FPCAR @ hex. FPU_FPCAR 1b. ;
    : FPU_FPSCR. cr ." FPU_FPSCR.  RW   $" FPU_FPSCR @ hex. FPU_FPSCR 1b. ;
    : FPU.
      FPU_FPCCR.
      FPU_FPCAR.
      FPU_FPSCR.
    ;
  [then]

  execute-defined? use-SCB [if]
    $E000ED00 constant SCB ( System control block ) 
    SCB $0 + constant SCB_CPUID ( read-only )  \ CPUID base register
    SCB $4 + constant SCB_ICSR ( read-write )  \ Interrupt control and state register
    SCB $8 + constant SCB_VTOR ( read-write )  \ Vector table offset register
    SCB $C + constant SCB_AIRCR ( read-write )  \ Application interrupt and reset control register
    SCB $10 + constant SCB_SCR ( read-write )  \ System control register
    SCB $14 + constant SCB_CCR ( read-write )  \ Configuration and control register
    SCB $18 + constant SCB_SHPR1 ( read-write )  \ System handler priority registers
    SCB $1C + constant SCB_SHPR2 ( read-write )  \ System handler priority registers
    SCB $20 + constant SCB_SHPR3 ( read-write )  \ System handler priority registers
    SCB $24 + constant SCB_SHCRS ( read-write )  \ System handler control and state register
    SCB $28 + constant SCB_CFSR_UFSR_BFSR_MMFSR ( read-write )  \ Configurable fault status register
    SCB $2C + constant SCB_HFSR ( read-write )  \ Hard fault status register
    SCB $34 + constant SCB_MMFAR ( read-write )  \ Memory management fault address register
    SCB $38 + constant SCB_BFAR ( read-write )  \ Bus fault address register
    : SCB_CPUID. cr ." SCB_CPUID.  RO   $" SCB_CPUID @ hex. SCB_CPUID 1b. ;
    : SCB_ICSR. cr ." SCB_ICSR.  RW   $" SCB_ICSR @ hex. SCB_ICSR 1b. ;
    : SCB_VTOR. cr ." SCB_VTOR.  RW   $" SCB_VTOR @ hex. SCB_VTOR 1b. ;
    : SCB_AIRCR. cr ." SCB_AIRCR.  RW   $" SCB_AIRCR @ hex. SCB_AIRCR 1b. ;
    : SCB_SCR. cr ." SCB_SCR.  RW   $" SCB_SCR @ hex. SCB_SCR 1b. ;
    : SCB_CCR. cr ." SCB_CCR.  RW   $" SCB_CCR @ hex. SCB_CCR 1b. ;
    : SCB_SHPR1. cr ." SCB_SHPR1.  RW   $" SCB_SHPR1 @ hex. SCB_SHPR1 1b. ;
    : SCB_SHPR2. cr ." SCB_SHPR2.  RW   $" SCB_SHPR2 @ hex. SCB_SHPR2 1b. ;
    : SCB_SHPR3. cr ." SCB_SHPR3.  RW   $" SCB_SHPR3 @ hex. SCB_SHPR3 1b. ;
    : SCB_SHCRS. cr ." SCB_SHCRS.  RW   $" SCB_SHCRS @ hex. SCB_SHCRS 1b. ;
    : SCB_CFSR_UFSR_BFSR_MMFSR. cr ." SCB_CFSR_UFSR_BFSR_MMFSR.  RW   $" SCB_CFSR_UFSR_BFSR_MMFSR @ hex. SCB_CFSR_UFSR_BFSR_MMFSR 1b. ;
    : SCB_HFSR. cr ." SCB_HFSR.  RW   $" SCB_HFSR @ hex. SCB_HFSR 1b. ;
    : SCB_MMFAR. cr ." SCB_MMFAR.  RW   $" SCB_MMFAR @ hex. SCB_MMFAR 1b. ;
    : SCB_BFAR. cr ." SCB_BFAR.  RW   $" SCB_BFAR @ hex. SCB_BFAR 1b. ;
    : SCB.
      SCB_CPUID.
      SCB_ICSR.
      SCB_VTOR.
      SCB_AIRCR.
      SCB_SCR.
      SCB_CCR.
      SCB_SHPR1.
      SCB_SHPR2.
      SCB_SHPR3.
      SCB_SHCRS.
      SCB_CFSR_UFSR_BFSR_MMFSR.
      SCB_HFSR.
      SCB_MMFAR.
      SCB_BFAR.
    ;
  [then]

  execute-defined? use-PF [if]
    $E000ED78 constant PF ( Processor features ) 
    PF $0 + constant PF_CLIDR ( read-only )  \ Cache Level ID register
    PF $4 + constant PF_CTR ( read-only )  \ Cache Type register
    PF $8 + constant PF_CCSIDR ( read-only )  \ Cache Size ID register
    : PF_CLIDR. cr ." PF_CLIDR.  RO   $" PF_CLIDR @ hex. PF_CLIDR 1b. ;
    : PF_CTR. cr ." PF_CTR.  RO   $" PF_CTR @ hex. PF_CTR 1b. ;
    : PF_CCSIDR. cr ." PF_CCSIDR.  RO   $" PF_CCSIDR @ hex. PF_CCSIDR 1b. ;
    : PF.
      PF_CLIDR.
      PF_CTR.
      PF_CCSIDR.
    ;
  [then]

  execute-defined? use-AC [if]
    $E000EF90 constant AC ( Access control ) 
    AC $0 + constant AC_ITCMCR ( read-write )  \ Instruction and Data Tightly-Coupled Memory Control Registers
    AC $4 + constant AC_DTCMCR ( read-write )  \ Instruction and Data Tightly-Coupled Memory Control Registers
    AC $8 + constant AC_AHBPCR ( read-write )  \ AHBP Control register
    AC $C + constant AC_CACR ( read-write )  \ Auxiliary Cache Control register
    AC $10 + constant AC_AHBSCR ( read-write )  \ AHB Slave Control register
    AC $18 + constant AC_ABFSR ( read-write )  \ Auxiliary Bus Fault Status register
    : AC_ITCMCR. cr ." AC_ITCMCR.  RW   $" AC_ITCMCR @ hex. AC_ITCMCR 1b. ;
    : AC_DTCMCR. cr ." AC_DTCMCR.  RW   $" AC_DTCMCR @ hex. AC_DTCMCR 1b. ;
    : AC_AHBPCR. cr ." AC_AHBPCR.  RW   $" AC_AHBPCR @ hex. AC_AHBPCR 1b. ;
    : AC_CACR. cr ." AC_CACR.  RW   $" AC_CACR @ hex. AC_CACR 1b. ;
    : AC_AHBSCR. cr ." AC_AHBSCR.  RW   $" AC_AHBSCR @ hex. AC_AHBSCR 1b. ;
    : AC_ABFSR. cr ." AC_ABFSR.  RW   $" AC_ABFSR @ hex. AC_ABFSR 1b. ;
    : AC.
      AC_ITCMCR.
      AC_DTCMCR.
      AC_AHBPCR.
      AC_CACR.
      AC_AHBSCR.
      AC_ABFSR.
    ;
  [then]

end-module

compile-to-ram
