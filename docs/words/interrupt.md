# Interrupt Words

### `interrupt`

These words are in `interrupt`.

##### `x-invalid-vector`
( -- )

Invalid interrupt vector index exception

##### `current-interrupt`
( -- interrupt )

Get the current interrupt (0 for none).

##### `in-interrupt?`
( -- in-interrupt? )

Get whether we are in an interrupt.

##### `vector!`
( xt vector-index -- )

Set an interrupt vector

##### `vector@`
( vector-index -- xt )

Get an interrupt vector

##### `SHPR1_PRI_4!`
( u -- )

Set system fault handler priority field 4, for memory management fault

##### `SHPR1_PRI_5!`
( u -- )

Set system fault handler priority field 5, for bus fault

##### `SHPR1_PRI_6!`
( u -- )

Set system fault handler priority field 6, for usage fault

##### `SHPR2_PRI_11!`
( u -- )

Set system fault handler priority field 11, for SVCall

##### `SHPR3_PRI_14!`
( u -- )

Set system fault handler priority field 14, for PendSV

##### `SHPR3_PRI_15!`
( u -- )

Set system fault handler priority field 15, for SysTick

##### `SHPR1_PRI_4@`
( -- u )

Get system fault handler priority field 4, for memory management fault

##### `SHPR1_PRI_5@`
( -- u )

Get system fault handler priority field 5, for bus fault

##### `SHPR1_PRI_6@`
( -- u )

Get system fault handler priority field 6, for usage fault

##### `SHPR2_PRI_11@`
( -- u )

Get system fault handler priority field 11, for SVCall

##### `SHPR3_PRI_14@`
( -- u )

Get system fault handler priority field 14, for PendSV

##### `SHPR3_PRI_15@`
( -- u )

Get system fault handler priority field 15, for SysTick

##### `ICSR_PENDSVSET!`
( -- )

Set PENDSVSET

##### `ICSR_PENDSVCLR!`
( -- )

Set PENDSVCLR

##### `ICSR_PENDSVSET@`
( -- bit )

Get PENDSVSET

##### `svc`
( -- )

Initiate an SVCall

##### `NVIC_ISER_SETENA!`
( u -- )

Set NVIC interrupt set-enable

##### `NVIC_ISER_SETENA@`
( u -- bit )

Get NVIC interrupt set-enable

##### `NVIC_ICER_CLRENA!`
( u -- )

Set NVIC interrupt clear-enable

##### `NVIC_ICER_CLRENA@`
( u -- bit )

Get NVIC interrupt clear-enable

##### `NVIC_ISPR_SETPEND!`
( u -- )

Set NVIC interrupt set-pending

##### `NVIC_ISPR_SETPEND@`
( u -- bit )

Get NVIC interrupt set-pending

##### `NVIC_ICPR_CLRPEND!`
( u -- )

Set NVIC interrupt clear-pending

##### `NVIC_ICPR_CLRPEND@`
( u -- bit )

Get NVIC interrupt clear-pending

##### `NVIC_IABR_ACTIVE@`
( u -- bit )

Get NVIC interrupt active bit

##### `NVIC_IPR_IP!`
( priority u -- )

Set NVIC interrupt priority register field

##### `NVIC_IPR_IP@`
( u -- priority )

Get NVIC interrupt priority register field
