        define_word "kernel-platform", visible_flag
_kernel_platform:
        push {lr}
        string "rp2040"
        pop {pc}
        end_inlined

        define_word "kernel-version", visible_flag
_kernel_version:
        push {lr}
        string "1.0.3-dev"
        pop {pc}
        end_inlined

        define_word "kernel-date", visible_flag
_kernel_date:
        push {lr}
        string "Sun Aug 20 18:36:15 CDT 2023"
        pop {pc}
        end_inlined

