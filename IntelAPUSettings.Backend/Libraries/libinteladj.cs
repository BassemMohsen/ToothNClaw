using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace IntelAPUSettings.Backend.Libraries
{
    public enum intel_family : int
    {
        WAIT_FOR_LOAD = -2,
        FAM_UNKNOWN = -1,
        FAM_RAVEN = 0,
        FAM_PICASSO,
        FAM_RENOIR,
        FAM_CEZANNE,
        FAM_DALI,
        FAM_LUCIENNE,
        FAM_VANGOGH,
        FAM_REMBRANDT,
        FAM_MENDOCINO,
        FAM_PHOENIX,
        FAM_HAWKPOINT,
        FAM_DRAGONRANGE,
        FAM_KRACKANPOINT,
        FAM_STRIXPOINT,
        FAM_STRIXHALO,
        FAM_FIRERANGE,
        FAM_END
    }

    public static class IntelAdjNative
    {
        private const string DllPath = "libinteladj.dll";

        public const int ADJ_ERR_FAM_UNSUPPORTED = -1;
        public const int ADJ_ERR_SMU_TIMEOUT = -2;
        public const int ADJ_ERR_SMU_UNSUPPORTED = -3;
        public const int ADJ_ERR_SMU_REJECTED = -4;
        public const int ADJ_ERR_MEMORY_ACCESS = -5;

        // init/cleanup
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern IntPtr init_inteladj();

        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern void cleanup_inteladj(IntPtr ry);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern intel_family get_cpu_family(IntPtr ry);

        [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int get_bios_if_ver(IntPtr ry);

        // Table
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int init_table(IntPtr ry);

        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern uint get_table_ver(IntPtr ry);

        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern UIntPtr get_table_size(IntPtr ry); // size_t

        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern IntPtr get_table_values(IntPtr ry); // float*

        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int refresh_table(IntPtr ry);

        // Setters
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int set_stapm_limit(IntPtr ry, uint value);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int set_fast_limit(IntPtr ry, uint value);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int set_slow_limit(IntPtr ry, uint value);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int set_slow_time(IntPtr ry, uint value);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int set_stapm_time(IntPtr ry, uint value);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int set_tctl_temp(IntPtr ry, uint value);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int set_vrm_current(IntPtr ry, uint value);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int set_vrmsoc_current(IntPtr ry, uint value);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int set_vrmgfx_current(IntPtr ry, uint value);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int set_vrmcvip_current(IntPtr ry, uint value);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int set_vrmmax_current(IntPtr ry, uint value);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int set_vrmgfxmax_current(IntPtr ry, uint value);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int set_vrmsocmax_current(IntPtr ry, uint value);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int set_psi0_current(IntPtr ry, uint value);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int set_psi3cpu_current(IntPtr ry, uint value);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int set_psi0soc_current(IntPtr ry, uint value);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int set_psi3gfx_current(IntPtr ry, uint value);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int set_max_gfxclk_freq(IntPtr ry, uint value);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int set_min_gfxclk_freq(IntPtr ry, uint value);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int set_max_socclk_freq(IntPtr ry, uint value);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int set_min_socclk_freq(IntPtr ry, uint value);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int set_max_fclk_freq(IntPtr ry, uint value);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int set_min_fclk_freq(IntPtr ry, uint value);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int set_max_vcn(IntPtr ry, uint value);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int set_min_vcn(IntPtr ry, uint value);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int set_max_lclk(IntPtr ry, uint value);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int set_min_lclk(IntPtr ry, uint value);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int set_prochot_deassertion_ramp(IntPtr ry, uint value);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int set_apu_skin_temp_limit(IntPtr ry, uint value);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int set_dgpu_skin_temp_limit(IntPtr ry, uint value);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int set_apu_slow_limit(IntPtr ry, uint value);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int set_skin_temp_power_limit(IntPtr ry, uint value);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int set_gfx_clk(IntPtr ry, uint value);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int set_oc_clk(IntPtr ry, uint value);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int set_per_core_oc_clk(IntPtr ry, uint value);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int set_oc_volt(IntPtr ry, uint value);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int set_disable_oc(IntPtr ry);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int set_enable_oc(IntPtr ry);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int set_power_saving(IntPtr ry);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int set_max_performance(IntPtr ry);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int set_coall(IntPtr ry, uint value);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int set_coper(IntPtr ry, uint value);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern int set_cogfx(IntPtr ry, uint value);

        // Getters
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern float get_stapm_limit(IntPtr ry);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern float get_stapm_value(IntPtr ry);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern float get_fast_limit(IntPtr ry);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern float get_fast_value(IntPtr ry);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern float get_slow_limit(IntPtr ry);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern float get_slow_value(IntPtr ry);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern float get_apu_slow_limit(IntPtr ry);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern float get_apu_slow_value(IntPtr ry);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern float get_vrm_current(IntPtr ry);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern float get_vrm_current_value(IntPtr ry);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern float get_vrmsoc_current(IntPtr ry);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern float get_vrmsoc_current_value(IntPtr ry);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern float get_vrmmax_current(IntPtr ry);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern float get_vrmmax_current_value(IntPtr ry);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern float get_vrmsocmax_current(IntPtr ry);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern float get_vrmsocmax_current_value(IntPtr ry);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern float get_tctl_temp(IntPtr ry);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern float get_tctl_temp_value(IntPtr ry);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern float get_apu_skin_temp_limit(IntPtr ry);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern float get_apu_skin_temp_value(IntPtr ry);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern float get_dgpu_skin_temp_limit(IntPtr ry);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern float get_dgpu_skin_temp_value(IntPtr ry);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern float get_psi0_current(IntPtr ry);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern float get_psi0soc_current(IntPtr ry);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern float get_stapm_time(IntPtr ry);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern float get_slow_time(IntPtr ry);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern float get_cclk_setpoint(IntPtr ry);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern float get_cclk_busy_value(IntPtr ry);

        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern float get_core_clk(IntPtr ry, uint coreIndex);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern float get_core_volt(IntPtr ry, uint coreIndex);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern float get_core_power(IntPtr ry, uint coreIndex);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern float get_core_temp(IntPtr ry, uint coreIndex);

        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern float get_l3_clk(IntPtr ry);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern float get_l3_logic(IntPtr ry);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern float get_l3_vddm(IntPtr ry);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern float get_l3_temp(IntPtr ry);

        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern float get_gfx_clk(IntPtr ry);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern float get_gfx_temp(IntPtr ry);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern float get_gfx_volt(IntPtr ry);

        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern float get_mem_clk(IntPtr ry);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern float get_fclk(IntPtr ry);

        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern float get_soc_power(IntPtr ry);
        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern float get_soc_volt(IntPtr ry);

        [DllImport(DllPath, CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        public static extern float get_socket_power(IntPtr ry);
    }
}
