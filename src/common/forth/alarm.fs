\ Copyright (c) 2023 Travis Bemann
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

begin-module alarm

  task import
  systick import
  lock import
  
  \ Default alarm task has already been initialized
  : x-default-alarm-task-already-inited ( -- )
    ." default alarm task has already been initialized" cr
  ;
  
  begin-module alarm-internal

    \ The default alarm task has been initialized
    variable default-alarm-task-inited?

    \ Default alarm task initialization lock
    lock-size aligned-buffer: default-alarm-task-lock
    
    begin-structure alarm-size

      \ Alarm handler
      field: alarm-xt

      \ Alarm data
      field: alarm-data
      
      \ Alarm start tick
      field: alarm-ticks-start

      \ Alarm delay tick
      field: alarm-ticks-delay

      \ Alarm priority
      field: alarm-priority

      \ Alarm parent task
      field: alarm-parent
      
      \ Previous alarm
      field: alarm-prev
      
      \ Next alarm
      field: alarm-next
      
    end-structure

    begin-structure alarm-task-size

      \ Alarm task
      field: alarm-task-task

      \ First alarm
      field: alarm-task-first

      \ Last alarm
      field: alarm-task-last

      \ Alarm lock
      lock-size +field alarm-task-lock

      \ Alarm notification space
      field: alarm-task-notify-data
      
    end-structure

    \ The default alarm task
    alarm-task-size aligned-buffer: default-alarm-task

    \ Get the maximum priority of all the alarms
    : max-alarm-priority ( alarm-task -- )
      [:
        0 { alarm-task max-priority }
        alarm-task alarm-task-first @ begin ?dup while
          dup alarm-priority @ max-priority max to max-priority
          alarm-next @
        repeat
        max-priority
      ;] over alarm-task-lock with-lock
    ;

    \ Compare two alarms times
    : alarm< { alarm1 alarm0 -- lt? }
      systick-counter { current-systick }
      current-systick alarm1 alarm-ticks-start @ - { adjusted-start1 }
      current-systick alarm0 alarm-ticks-start @ - { adjusted-start0 }
      alarm1 alarm-ticks-delay @ adjusted-start1 - { adjusted-delay1 }
      alarm0 alarm-ticks-delay @ adjusted-start0 - { adjusted-delay0 }
      adjusted-delay1 adjusted-delay0 <
    ;
    
    \ Add an alarm
    : add-alarm ( alarm alarm-task -- )
      [: { alarm alarm-task }
        alarm-task alarm-task-first @ begin ?dup while
          dup alarm-prev @ { prev }
          alarm over alarm< if
            prev if
              prev alarm alarm-prev !
              alarm prev alarm-next !
            else
              alarm alarm-task alarm-task-first !
              0 alarm alarm-prev !
            then
            alarm alarm-next !
            alarm-task alarm alarm-parent !
            exit
          else
            alarm-next @
          then
        repeat
        alarm-task alarm-task-last @ { last }
        last if
          last alarm alarm-prev !
          alarm last alarm-next !
        else
          alarm alarm-task alarm-task-first !
          0 alarm alarm-prev !
        then
        0 alarm alarm-next !
        alarm alarm-task alarm-task-last !
        alarm-task alarm alarm-parent !
      ;] over alarm-task-lock with-lock
    ;

    \ Remove an alarm
    : remove-alarm { alarm -- }
      alarm alarm-parent @ if
        alarm [: { alarm }
          alarm alarm-prev @ if
            alarm alarm-next @ alarm alarm-prev @ alarm-next !
          else
            alarm alarm-next @ alarm alarm-parent @ alarm-task-first !
          then
          alarm alarm-next @ if
            alarm alarm-prev @ alarm alarm-next @ alarm-prev !
          else
            alarm alarm-prev @ alarm alarm-parent @ alarm-task-last !
          then
        ;] alarm alarm-parent @ alarm-task-lock with-lock
      then
      0 alarm alarm-parent !
    ;

    \ Run an alarm task
    : run-alarm-task { alarm-task -- }
      begin
        alarm-task [:
          alarm-task-first @
          dup if
            dup alarm-ticks-delay @
            over alarm-ticks-start @
            true
          then
        ;] alarm-task alarm-task-lock with-lock
        if
          dup systick-counter swap - 2 pick < if
            rot drop
            [: 2dup 0 wait-notify-timeout drop ;] try
            -rot 2drop dup ['] x-timed-out <> if ?raise else drop then
          else
            2drop dup alarm-data @ swap dup alarm-xt @
            over remove-alarm
            execute
            alarm-task max-alarm-priority current-task task-priority!
          then
        else
          0 wait-notify-indefinite drop
        then
      again
    ;

    \ Initialize alarms
    : init-alarms ( -- )
      default-alarm-task-lock init-lock
      false default-alarm-task-inited? !
    ;

    \ Default default alarm task dictionary size
    320 constant default-dict-size

    \ Default default alarm task stack size
    128 constant default-stack-size

    \ Default default alarm task return stack size
    512 constant default-rstack-size

    \ Default default alarm task core
    0 constant default-core
    
  end-module> import
  
  \ Initialize an alarm task
  : init-alarm-task { dict-size stack-size rstack-size core addr -- }
    addr alarm-task-lock init-lock
    0 addr alarm-task-first !
    0 addr alarm-task-last !
    addr 1 ['] run-alarm-task dict-size stack-size rstack-size core
    spawn-on-core addr alarm-task-task !
    addr alarm-task-notify-data 1 addr alarm-task-task @ config-notify
    c" alarm" addr alarm-task-task @ task-name!
    addr alarm-task-task @ run
  ;

  \ Initialize the default alarm task
  : init-default-alarm-task ( dict-size stack-size rstack-size core -- )
    [: { dict-size stack-size rstack-size core }
      default-alarm-task-inited? @ triggers x-default-alarm-task-already-inited
      true default-alarm-task-inited? !
      dict-size stack-size rstack-size core default-alarm-task init-alarm-task
    ;] default-alarm-task-lock with-lock
  ;

  \ Get the default alarm task
  : default-alarm-task@ ( -- default-alarm-task )
    [:
      default-alarm-task-inited? @ not if
        default-dict-size default-stack-size default-rstack-size default-core
        init-default-alarm-task
      then
      default-alarm-task
    ;] default-alarm-task-lock with-lock
  ;
  
  \ Set an alarm
  : set-alarm { ticks-delay ticks-start priority data xt alarm alarm-task -- }
    xt alarm alarm-xt !
    data alarm alarm-data !
    priority alarm alarm-priority !
    ticks-start alarm alarm-ticks-start !
    ticks-delay alarm alarm-ticks-delay !
    alarm alarm-task add-alarm
    current-task task-priority@ { saved-priority }
    alarm-task max-alarm-priority { target-priority }
    target-priority current-task task-priority!
    target-priority alarm-task alarm-task-task @ task-priority!
    0 alarm-task alarm-task-task @ notify
    saved-priority current-task task-priority!
  ;

  \ Set an alarm with default alarm task
  : set-alarm-default ( ticks-delay ticks-start priority data xt alarm -- )
    default-alarm-task@ set-alarm
  ;

  \ Set an alarm for a delay
  : set-alarm-delay { ticks-delay priority data xt alarm alarm-task -- }
    ticks-delay systick-counter priority data xt alarm alarm-task set-alarm
  ;

  \ Set an alarm for a delay with default alarm task
  : set-alarm-delay-default { ticks-delay priority data xt alarm -- }
    ticks-delay systick-counter priority data xt alarm set-alarm-default
  ;

  \ Unset an alarm
  : unset-alarm { alarm -- }
    alarm alarm-parent @ { alarm-task }
    alarm-task if
      alarm remove-alarm
      alarm-task max-alarm-priority alarm-task alarm-task-task @ task-priority!
      0 alarm-task alarm-task-task @ notify
    then
  ;

  \ Alarm size
  ' alarm-size export alarm-size
  
end-module

\ Initialize
: init ( -- ) init alarm::alarm-internal::init-alarms ;

reboot