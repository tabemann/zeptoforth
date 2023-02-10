        define_word "kernel-platform", visible_flag
_kernel_platform:
        push {lr}
        string "rp2040"
        pop {pc}
        end_inlined

        define_word "kernel-version", visible_flag
_kernel_version:
        push {lr}
        string "0.60.1-dev"
        pop {pc}
        end_inlined

        define_word "kernel-date", visible_flag
_kernel_date:
        push {lr}
        string "Thu Feb 9 18:05:06 PST 2023"
        pop {pc}
        end_inlined

