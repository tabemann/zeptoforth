\ Copyright (c) 2021 Travis Bemann
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

\ Compile this to flash
compile-to-flash

begin-module-once gpio-module

  \ GPIO base
  $48000000 constant GPIO_Base

  \ GPIO index base
  : GPIO_Index ( index "name" -- )
    $400 * GPIO_Base + constant \
  ;

  \ The GPIOs
  0 GPIO_Index GPIOA
  1 GPIO_Index GPIOB
  2 GPIO_Index GPIOC
  3 GPIO_Index GPIOD
  4 GPIO_Index GPIOE
  5 GPIO_Index GPIOF
  6 GPIO_Index GPIOG
  7 GPIO_Index GPIOH
  8 GPIO_Index GPIOI
  
  \ A GPIO field
  : GPIO_Field ( offset "name" -- )
    <builds , does> @ +
  ;
  
  \ The GPIO fields
  $00 GPIO_Field MODER
  $04 GPIO_Field OTYPER
  $08 GPIO_Field OSPEEDR
  $0C GPIO_Field PUPDR
  $10 GPIO_Field IDR
  $14 GPIO_Field ODR
  $18 GPIO_Field BSRR
  $1C GPIO_Field LCKR
  $20 GPIO_Field AFRL
  $24 GPIO_Field AFRH

  \ The GPIO port modes
  %00 constant INPUT_MODE
  %01 constant OUTPUT_MODE
  %10 constant ALTERNATE_MODE
  %11 constant ANALOG_MODE

  \ The GPIO port output types
  %0 constant PUSH_PULL
  %1 constant OPEN_DRAIN

  \ The GPIO port output speeds
  %00 constant LOW_SPEED
  %01 constant MEDIUM_SPEED
  %10 constant HIGH_SPEED
  %11 constant VERY_HIGH_SPEED

  \ The GPIO port pull-up/pull-down settings
  %00 constant NO_PULL_UP_PULL_DOWN
  %01 constant PULL_UP
  %10 constant PULL_DOWN

  \ RCC base
  $40021000 constant RCC_Base

  \ RCC AHB1 peripheral clock enable register
  RCC_Base $4C + constant RCC_AHB2ENR
  
  \ RCC AHB1 peripheral clock enable in low-power mode register
  RCC_Base $6C + constant RCC_AHB2SMENR

  \ Enable a GPIO peripheral clock
  : gpio-clock-enable ( gpio -- )
    GPIO_Base - $400 / 1 swap lshift RCC_AHB2ENR bis!
  ;

  \ Enable a low-power GPIO peripheral clock
  : gpio-lp-clock-enable ( gpio -- )
    GPIO_Base - $400 / 1 swap lshift RCC_AHB2SMENR bis!
  ;

  \ Enable a GPIO peripheral clock
  : gpio-clock-disable ( gpio -- )
    GPIO_Base - $400 / 1 swap lshift RCC_AHB2ENR bic!
  ;

  \ Enable a low-power GPIO peripheral clock
  : gpio-lp-clock-disable ( gpio -- )
    GPIO_Base - $400 / 1 swap lshift RCC_AHB2SMENR bic!
  ;

  \ Set a GPIOx_MODER field
  : MODER! ( mode port gpio -- )
    MODER dup >r @ over %11 swap 1 lshift lshift bic
    rot %11 and rot 1 lshift lshift or r> !
  ;

  \ Set a GPIOx_OTYPER field
  : OTYPER! ( otype port gpio -- )
    OTYPER dup >r @ over %1 swap lshift bic
    rot %1 and rot lshift or r> !
  ;

  \ Set a GPIOx_OSPEEDR field
  : OSPEEDR! ( ospeed port gpio -- )
    OSPEEDR dup >r @ over %11 swap 1 lshift lshift bic
    rot %11 and rot 1 lshift lshift or r> !
  ;

  \ Set a GPIOx_PUPDR field
  : PUPDR! ( pupd port gpio -- )
    PUPDR dup >r @ over %11 swap 1 lshift lshift bic
    rot %11 and rot 1 lshift lshift or r> !
  ;

  \ Set a GPIOx_AFRL field
  : AFRL! ( af port gpio -- )
    AFRL dup >r @ over %1111 swap 2 lshift lshift bic
    rot %1111 and rot 2 lshift lshift or r> !
  ;

  \ Set a GPIOx_AFRH field
  : AFRH! ( af port gpio -- )
    swap 8 - swap AFRH dup >r @ over %1111 swap 2 lshift lshift bic
    rot %1111 and rot 2 lshift lshift or r> !
  ;

  \ Set either a GPIOx_AFRL field or a GPIOx_AFRH field
  : AFR! ( af port gpio -- )
    over 8 >= if AFRH! else AFRL! then
  ;

  \ Set a single bit on a GPIO port
  : BS! ( port gpio -- )
    BSRR 1 rot lshift swap !
  ;

  \ Reset a single bit on a GPIO port
  : BR! ( port gpio -- )
    BSRR 1 rot 16 + lshift swap !
  ;

  \ Set or reset a single bit on a GPIO port
  : BSRR! ( bit port gpio -- )
    rot if BS! else BR! then
  ;

  \ Get a GPIOx_MODER field
  : MODER@ ( port gpio -- mode )
    MODER @ swap 1 lshift rshift %11 and
  ;

  \ Get a GPIOx_OTYPER field
  : OTYPER@ ( port gpio -- otype )
    OTYPER @ swap rshift %1 and
  ;

  \ Get a GPIOx_OSPEEDR field
  : OSPEEDR@ ( port gpio -- ospeed )
    OSPEEDR @ swap 1 lshift rshift %11 and
  ;

  \ Get a GPIOx_OSPEEDR field
  : PUPDR@ ( port gpio -- pupd )
    PUPDR @ swap 1 lshift rshift %11 and
  ;

  \ Get a GPIOx_AFRL field
  : AFRL@ ( port gpio -- af )
    AFRL @ swap 2 lshift rshift %1111 and
  ;

  \ Get a GPIOx_AFRH field
  : AFRH@ ( port gpio -- af )
    AFRH @ swap 8 - 2 lshift rshift %1111 and
  ;

  \ Get either a GPIOx_AFRL field or a GPIOx_AFRH field
  : AFR@ ( port gpio -- af )
    over 8 >= if AFRH@ else AFRL@ then
  ;

  \ Get an input for an GPIO port
  : IDR@ ( port gpio -- input )
    IDR @ swap rshift %1 and 0<>
  ;

end-module