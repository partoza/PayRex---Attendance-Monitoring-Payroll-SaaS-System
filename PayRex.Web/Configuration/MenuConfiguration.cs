namespace PayRex.Web.Configuration
{
    using PayRex.Web.Models;

    /// <summary>
    /// Static configuration for application menu items with role-based access.
    /// 
    /// Two view modes exist for dual-role users (HR, Accountant):
    ///   - Management View (default): shows management/admin pages for their role
    ///   - Employee View: shows employee-only pages scoped to their personal data
    /// 
    /// Pure Employee users always see the Employee menu.
    /// Admin users always see the Management menu (no Employee View toggle).
    /// </summary>
    public static class MenuConfiguration
    {
        // ── SVG icon constants ──
        private const string IconDashboard = @"<svg class=""shrink-0 w-5 h-5 transition duration-75 group-hover:text-fg-brand"" xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""currentColor""><path d=""M11.47 3.841a.75.75 0 0 1 1.06 0l8.69 8.69a.75.75 0 1 0 1.06-1.061l-8.689-8.69a2.25 2.25 0 0 0-3.182 0l-8.69 8.69a.75.75 0 1 0 1.061 1.06l8.69-8.689Z"" /><path d=""m12 5.432 8.159 8.159c.03.03.06.058.091.086v6.198c0 1.035-.84 1.875-1.875 1.875H15a.75.75 0 0 1-.75-.75v-4.5a.75.75 0 0 0-.75-.75h-3a.75.75 0 0 0-.75.75V21a.75.75 0 0 1-.75.75H5.625a1.875 1.875 0 0 1-1.875-1.875v-6.198a2.29 2.29 0 0 0 .091-.086L12 5.432Z"" /></svg>";
        private const string IconUsers = @"<svg class=""shrink-0 w-5 h-5 transition duration-75 group-hover:text-fg-brand"" xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""currentColor""><path fill-rule=""evenodd"" d=""M8.25 6.75a3.75 3.75 0 1 1 7.5 0 3.75 3.75 0 0 1-7.5 0ZM15.75 9.75a3 3 0 1 1 6 0 3 3 0 0 1-6 0ZM2.25 9.75a3 3 0 1 1 6 0 3 3 0 0 1-6 0ZM6.31 15.117A6.745 6.745 0 0 1 12 12a6.745 6.745 0 0 1 6.709 7.498.75.75 0 0 1-.372.568A12.696 12.696 0 0 1 12 21.75c-2.305 0-4.47-.612-6.337-1.684a.75.75 0 0 1-.372-.568 6.787 6.787 0 0 1 1.019-4.38Z"" clip-rule=""evenodd"" /><path d=""M5.082 14.254a8.287 8.287 0 0 0-1.308 5.135 9.687 9.687 0 0 1-1.764-.44l-.115-.04a.563.563 0 0 1-.373-.487l-.01-.121a3.75 3.75 0 0 1 3.57-4.047ZM20.226 19.389a8.287 8.287 0 0 0-1.308-5.135 3.75 3.75 0 0 1 3.57 4.047l-.01.121a.563.563 0 0 1-.373.486l-.115.04c-.567.2-1.156.349-1.764.441Z"" /></svg>";
        private const string IconUserMgmt = @"<svg class=""shrink-0 w-5 h-5 transition duration-75 group-hover:text-fg-brand"" xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""currentColor""><path fill-rule=""evenodd"" d=""M8.25 6.75a3.75 3.75 0 1 1 7.5 0 3.75 3.75 0 0 1-7.5 0ZM15.75 9.75a3 3 0 1 1 6 0 3 3 0 0 1-6 0ZM2.25 9.75a3 3 0 1 1 6 0 3 3 0 0 1-6 0ZM6.31 15.117A6.745 6.745 0 0 1 12 12a6.745 6.745 0 0 1 6.709 7.498.75.75 0 0 1-.372.568A12.696 12.696 0 0 1 12 21.75c-2.305 0-4.47-.612-6.337-1.684a.75.75 0 0 1-.372-.568 6.787 6.787 0 0 1 1.019-4.38Z"" clip-rule=""evenodd"" /><path d=""M5.082 14.254a8.287 8.287 0 0 0-1.308 5.135 9.687 9.687 0 0 1-1.764-.44l-.115-.04a.563.563 0 0 1-.373-.487l-.01-.121a3.75 3.75 0 0 1 3.57-4.047ZM20.226 19.389a8.287 8.287 0 0 0-1.308-5.135 3.75 3.75 0 0 1 3.57 4.047l-.01.121a.563.563 0 0 1-.373.486l-.115.04c-.567.2-1.156.349-1.764.441Z"" /></svg>";
        private const string IconClock = @"<svg class=""shrink-0 w-5 h-5 transition duration-75 group-hover:text-fg-brand"" xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""currentColor""><path fill-rule=""evenodd"" d=""M12 2.25c-5.385 0-9.75 4.365-9.75 9.75s4.365 9.75 9.75 9.75 9.75-4.365 9.75-9.75S17.385 2.25 12 2.25ZM12.75 6a.75.75 0 0 0-1.5 0v6c0 .414.336.75.75.75h4.5a.75.75 0 0 0 0-1.5h-3.75V6Z"" clip-rule=""evenodd"" /></svg>";
        private const string IconCalendar = @"<svg class=""shrink-0 w-5 h-5 transition duration-75 group-hover:text-fg-brand"" xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""currentColor""><path fill-rule=""evenodd"" d=""M6.75 2.25A.75.75 0 0 1 7.5 3v1.5h9V3A.75.75 0 0 1 18 3v1.5h.75a3 3 0 0 1 3 3v11.25a3 3 0 0 1-3 3H5.25a3 3 0 0 1-3-3V7.5a3 3 0 0 1 3-3H6V3a.75.75 0 0 1 .75-.75Zm13.5 9a1.5 1.5 0 0 0-1.5-1.5H5.25a1.5 1.5 0 0 0-1.5 1.5v7.5a1.5 1.5 0 0 0 1.5 1.5h13.5a1.5 1.5 0 0 0 1.5-1.5v-7.5Z"" clip-rule=""evenodd"" /></svg>";
        private const string IconSalary = @"<svg class=""shrink-0 w-5 h-5 transition duration-75 group-hover:text-fg-brand"" xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""currentColor""><path d=""M12 7.5a2.25 2.25 0 1 0 0 4.5 2.25 2.25 0 0 0 0-4.5Z"" /><path fill-rule=""evenodd"" d=""M1.5 4.875C1.5 3.839 2.34 3 3.375 3h17.25c1.035 0 1.875.84 1.875 1.875v9.75c0 1.036-.84 1.875-1.875 1.875H3.375A1.875 1.875 0 0 1 1.5 14.625v-9.75ZM8.25 9.75a3.75 3.75 0 1 1 7.5 0 3.75 3.75 0 0 1-7.5 0ZM18.75 9a.75.75 0 0 0-.75.75v.008c0 .414.336.75.75.75h.008a.75.75 0 0 0 .75-.75V9.75a.75.75 0 0 0-.75-.75h-.008ZM4.5 9.75A.75.75 0 0 1 5.25 9h.008a.75.75 0 0 1 .75.75v.008a.75.75 0 0 1-.75.75H5.25a.75.75 0 0 1-.75-.75V9.75Z"" clip-rule=""evenodd"" /><path d=""M2.25 18a.75.75 0 0 0 0 1.5c5.4 0 10.63.722 15.6 2.075 1.19.324 2.4-.558 2.4-1.82V18.75a.75.75 0 0 0-.75-.75H2.25Z"" /></svg>";
        private const string IconCard = @"<svg class=""shrink-0 w-5 h-5 transition duration-75 group-hover:text-fg-brand"" xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""currentColor""><path d=""M4.5 3.75a3 3 0 0 0-3 3v.75h21v-.75a3 3 0 0 0-3-3h-15Z"" /><path fill-rule=""evenodd"" d=""M22.5 9.75h-21v7.5a3 3 0 0 0 3 3h15a3 3 0 0 0 3-3v-7.5Zm-18 3.75a.75.75 0 0 1 .75-.75h6a.75.75 0 0 1 0 1.5h-6a.75.75 0 0 1-.75-.75Zm.75 2.25a.75.75 0 0 0 0 1.5h3a.75.75 0 0 0 0-1.5h-3Z"" clip-rule=""evenodd"" /></svg>";
        private const string IconFinance = @"<svg class=""shrink-0 w-5 h-5 transition duration-75 group-hover:text-fg-brand"" xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""currentColor""><path d=""M10.464 8.746c.227-.18.497-.311.786-.394v2.795a2.252 2.252 0 0 1-.786-.393c-.394-.313-.546-.681-.546-1.004 0-.323.152-.691.546-1.004ZM12.75 15.662v-2.824c.347.085.664.228.921.421.427.32.579.686.579.991 0 .305-.152.671-.579.991a2.534 2.534 0 0 1-.921.42Z"" /><path fill-rule=""evenodd"" d=""M12 2.25c-5.385 0-9.75 4.365-9.75 9.75s4.365 9.75 9.75 9.75 9.75-4.365 9.75-9.75S17.385 2.25 12 2.25ZM12.75 6a.75.75 0 0 0-1.5 0v.816a3.836 3.836 0 0 0-1.72.756c-.712.566-1.112 1.35-1.112 2.178 0 .829.4 1.612 1.113 2.178.502.4 1.102.647 1.719.756v2.978a2.536 2.536 0 0 1-.921-.421l-.879-.66a.75.75 0 0 0-.9 1.2l.879.66c.533.4 1.169.645 1.821.75V18a.75.75 0 0 0 1.5 0v-.81a4.124 4.124 0 0 0 1.821-.749c.745-.559 1.179-1.344 1.179-2.191 0-.847-.434-1.632-1.179-2.191a4.122 4.122 0 0 0-1.821-.75V8.354c.29.082.559.213.786.393l.415.33a.75.75 0 0 0 .933-1.175l-.415-.33a3.836 3.836 0 0 0-1.719-.755V6Z"" clip-rule=""evenodd"" /></svg>";
        private const string IconCompensation = @"<svg class=""shrink-0 w-5 h-5 transition duration-75 group-hover:text-fg-brand"" xmlns=""http://www.w3.org/2000/svg"" width=""24"" height=""24"" fill=""none"" viewBox=""0 0 24 24""><path fill=""currentColor"" d=""M4 19v2c0 .5523.44772 1 1 1h14c.5523 0 1-.4477 1-1v-2H4Z""/><path fill=""currentColor"" fill-rule=""evenodd"" d=""M9 3c0-.55228.44772-1 1-1h8c.5523 0 1 .44772 1 1v3c0 .55228-.4477 1-1 1h-2v1h2c.5096 0 .9376.38314.9939.88957L19.8951 17H4.10498l.90116-8.11043C5.06241 8.38314 5.49047 8 6.00002 8H12V7h-2c-.55228 0-1-.44772-1-1V3Zm1.01 8H8.00002v2.01H10.01V11Zm.99 0h2.01v2.01H11V11Zm5.01 0H14v2.01h2.01V11Zm-8.00998 3H10.01v2.01H8.00002V14ZM13.01 14H11v2.01h2.01V14Zm.99 0h2.01v2.01H14V14ZM11 4h6v1h-6V4Z"" clip-rule=""evenodd""/></svg>";
        private const string IconPayslip = @"<svg class=""shrink-0 w-5 h-5 transition duration-75 group-hover:text-fg-brand"" xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""currentColor""><path fill-rule=""evenodd"" d=""M3.75 3.375c0-1.036.84-1.875 1.875-1.875H9a3.75 3.75 0 0 1 3.75 3.75v1.875c0 1.036.84 1.875 1.875 1.875H16.5a3.75 3.75 0 0 1 3.75 3.75v7.875c0 1.035-.84 1.875-1.875 1.875H5.625a1.875 1.875 0 0 1-1.875-1.875V3.375Zm10.5 1.875a5.23 5.23 0 0 0-1.279-3.434 9.768 9.768 0 0 1 6.963 6.963A5.23 5.23 0 0 0 16.5 7.5h-1.875a.375.375 0 0 1-.375-.375V5.25ZM12 10.5a.75.75 0 0 1 .75.75v.028a9.727 9.727 0 0 1 1.687.28.75.75 0 1 1-.374 1.452 8.207 8.207 0 0 0-1.313-.226v1.68l.969.332c.67.23 1.281.85 1.281 1.704 0 .158-.007.314-.02.468-.083.931-.83 1.582-1.669 1.695a9.776 9.776 0 0 1-.561.059v.028a.75.75 0 0 1-1.5 0v-.029a9.724 9.724 0 0 1-1.687-.278.75.75 0 0 1 .374-1.453c.425.11.864.186 1.313.226v-1.68l-.968-.332C9.612 14.974 9 14.354 9 13.5c0-.158.007-.314.02-.468.083-.931.831-1.582 1.67-1.694.185-.025.372-.045.56-.06v-.028a.75.75 0 0 1 .75-.75Zm-1.11 2.324c.119-.016.239-.03.36-.04v1.166l-.482-.165c-.208-.072-.268-.211-.268-.285 0-.113.005-.225.015-.336.013-.146.14-.309.374-.34Zm1.86 4.392V16.05l.482.165c.208.072.268.211.268.285 0 .113-.005.225-.015.336-.012.146-.14.309-.374.34-.12.016-.24.03-.361.04Z"" clip-rule=""evenodd"" /></svg>";
        private const string IconAudit = @"<svg class=""shrink-0 w-5 h-5 transition duration-75 group-hover:text-fg-brand"" xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""currentColor""><path fill-rule=""evenodd"" d=""M7.502 6h7.128A3.375 3.375 0 0 1 18 9.375v9.375a3 3 0 0 0 3-3V6.108c0-1.505-1.125-2.811-2.664-2.94a48.972 48.972 0 0 0-.673-.05A3 3 0 0 0 15 1.5h-1.5a3 3 0 0 0-2.663 1.618c-.225.015-.45.032-.673.05C8.662 3.295 7.554 4.542 7.502 6ZM13.5 3A1.5 1.5 0 0 0 12 4.5h4.5A1.5 1.5 0 0 0 15 3h-1.5Z"" clip-rule=""evenodd"" /><path fill-rule=""evenodd"" d=""M3 9.375C3 8.339 3.84 7.5 4.875 7.5h9.75c1.036 0 1.875.84 1.875 1.875v11.25c0 1.035-.84 1.875-1.875 1.875h-9.75A1.875 1.875 0 0 1 3 20.625V9.375ZM6 12a.75.75 0 0 1 .75-.75h.008a.75.75 0 0 1 .75.75v.008a.75.75 0 0 1-.75.75H6.75a.75.75 0 0 1-.75-.75V12Zm2.25 0a.75.75 0 0 1 .75-.75h3.75a.75.75 0 0 1 0 1.5H9a.75.75 0 0 1-.75-.75ZM6 15a.75.75 0 0 1 .75-.75h.008a.75.75 0 0 1 .75.75v.008a.75.75 0 0 1-.75.75H6.75a.75.75 0 0 1-.75-.75V15Zm2.25 0a.75.75 0 0 1 .75-.75h3.75a.75.75 0 0 1 0 1.5H9a.75.75 0 0 1-.75-.75ZM6 18a.75.75 0 0 1 .75-.75h.008a.75.75 0 0 1 .75.75v.008a.75.75 0 0 1-.75.75H6.75a.75.75 0 0 1-.75-.75V18Zm2.25 0a.75.75 0 0 1 .75-.75h3.75a.75.75 0 0 1 0 1.5H9a.75.75 0 0 1-.75-.75Z"" clip-rule=""evenodd"" /></svg>";
        private const string IconSettings = @"<svg class=""shrink-0 w-5 h-5 transition duration-75 group-hover:text-fg-brand"" xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""currentColor""><path fill-rule=""evenodd"" d=""M11.078 2.25c-.917 0-1.699.663-1.85 1.567L9.05 4.889c-.02.12-.115.26-.297.348a7.493 7.493 0 0 0-.986.57c-.166.115-.334.126-.45.083L6.3 5.508a1.875 1.875 0 0 0-2.282.819l-.922 1.597a1.875 1.875 0 0 0 .432 2.385l.84.692c.095.078.17.229.154.43a7.598 7.598 0 0 0 0 1.139c.015.2-.059.352-.153.43l-.841.692a1.875 1.875 0 0 0-.432 2.385l.922 1.597a1.875 1.875 0 0 0 2.282.818l1.019-.382c.115-.043.283-.031.45.082.312.214.641.405.985.57.182.088.277.228.297.35l.178 1.071c.151.904.933 1.567 1.85 1.567h1.844c.916 0 1.699-.663 1.85-1.567l.178-1.072c.02-.12.114-.26.297-.349.344-.165.673-.356.985-.57.167-.114.335-.125.45-.082l1.02.382a1.875 1.875 0 0 0 2.28-.819l.923-1.597a1.875 1.875 0 0 0-.432-2.385l-.84-.692c-.095-.078-.17-.229-.154-.43a7.614 7.614 0 0 0 0-1.139c-.016-.2.059-.352.153-.43l.84-.692c.708-.582.891-1.59.433-2.385l-.922-1.597a1.875 1.875 0 0 0-2.282-.818l-1.02.382c-.114.043-.282.031-.449-.083a7.49 7.49 0 0 0-.985-.57c-.183-.087-.277-.227-.297-.348l-.179-1.072a1.875 1.875 0 0 0-1.85-1.567h-1.843ZM12 15.75a3.75 3.75 0 1 0 0-7.5 3.75 3.75 0 0 0 0 7.5Z"" clip-rule=""evenodd"" /></svg>";
        private const string IconBuilding = @"<svg class=""shrink-0 w-5 h-5 transition duration-75 group-hover:text-fg-brand"" xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""currentColor""><path fill-rule=""evenodd"" d=""M3 2.25a.75.75 0 0 0 0 1.5v16.5h-.75a.75.75 0 0 0 0 1.5H15v-18a.75.75 0 0 0 0-1.5H3ZM6.75 19.5v-2.25a.75.75 0 0 1 .75-.75h3a.75.75 0 0 1 .75.75v2.25a.75.75 0 0 1-.75.75h-3a.75.75 0 0 1-.75-.75ZM6 6.75A.75.75 0 0 1 6.75 6h.75a.75.75 0 0 1 0 1.5h-.75A.75.75 0 0 1 6 6.75ZM6.75 9a.75.75 0 0 0 0 1.5h.75a.75.75 0 0 0 0-1.5h-.75ZM6 12.75a.75.75 0 0 1 .75-.75h.75a.75.75 0 0 1 0 1.5h-.75a.75.75 0 0 1-.75-.75ZM10.5 6a.75.75 0 0 0 0 1.5h.75a.75.75 0 0 0 0-1.5h-.75Zm-.75 3.75A.75.75 0 0 1 10.5 9h.75a.75.75 0 0 1 0 1.5h-.75a.75.75 0 0 1-.75-.75ZM10.5 12a.75.75 0 0 0 0 1.5h.75a.75.75 0 0 0 0-1.5h-.75ZM16.5 6.75v15h5.25a.75.75 0 0 0 0-1.5H21v-12a.75.75 0 0 0 0-1.5h-4.5Zm1.5 4.5a.75.75 0 0 1 .75-.75h.008a.75.75 0 0 1 .75.75v.008a.75.75 0 0 1-.75.75h-.008a.75.75 0 0 1-.75-.75v-.008Zm.75 2.25a.75.75 0 0 0-.75.75v.008c0 .414.336.75.75.75h.008a.75.75 0 0 0 .75-.75v-.008a.75.75 0 0 0-.75-.75h-.008ZM18 17.25a.75.75 0 0 1 .75-.75h.008a.75.75 0 0 1 .75.75v.008a.75.75 0 0 1-.75.75h-.008a.75.75 0 0 1-.75-.75v-.008Z"" clip-rule=""evenodd"" /></svg>";
        private const string IconDocument = @"<svg class=""shrink-0 w-5 h-5 transition duration-75 group-hover:text-fg-brand"" xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""currentColor""><path d=""M7.5 3.375c0-1.036.84-1.875 1.875-1.875h.375a3.75 3.75 0 0 1 3.75 3.75v1.875C13.5 8.161 14.34 9 15.375 9h1.875A3.75 3.75 0 0 1 21 12.75v3.375C21 17.16 20.16 18 19.125 18h-9.75A1.875 1.875 0 0 1 7.5 16.125V3.375Z"" /><path d=""M15 5.25a5.23 5.23 0 0 0-1.279-3.434 9.768 9.768 0 0 1 6.963 6.963A5.23 5.23 0 0 0 17.25 7.5h-1.875A.375.375 0 0 1 15 7.125V5.25ZM4.875 6H6v10.125A3.375 3.375 0 0 0 9.375 19.5H16.5v1.125c0 1.035-.84 1.875-1.875 1.875h-9.75A1.875 1.875 0 0 1 3 20.625V7.875C3 6.839 3.84 6 4.875 6Z"" /></svg>";
        private const string IconArchive = @"<svg class=""shrink-0 w-5 h-5 transition duration-75 group-hover:text-fg-brand"" xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""currentColor""><path d=""M3.375 3C2.339 3 1.5 3.84 1.5 4.875v.75c0 1.036.84 1.875 1.875 1.875h17.25c1.035 0 1.875-.84 1.875-1.875v-.75C22.5 3.839 21.66 3 20.625 3H3.375Z"" /><path fill-rule=""evenodd"" d=""m3.087 9 .54 9.176A3 3 0 0 0 6.62 21h10.757a3 3 0 0 0 2.995-2.824L20.913 9H3.087Zm6.163 3.75A.75.75 0 0 1 10 12h4a.75.75 0 0 1 0 1.5h-4a.75.75 0 0 1-.75-.75Z"" clip-rule=""evenodd"" /></svg>";
        private const string IconQr = @"<svg class=""shrink-0 w-5 h-5 transition duration-75 group-hover:text-fg-brand"" xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""currentColor""><path fill-rule=""evenodd"" d=""M3 4.875C3 3.839 3.84 3 4.875 3h4.5c1.036 0 1.875.84 1.875 1.875v4.5c0 1.036-.84 1.875-1.875 1.875h-4.5A1.875 1.875 0 0 1 3 9.375v-4.5ZM4.875 4.5a.375.375 0 0 0-.375.375v4.5c0 .207.168.375.375.375h4.5a.375.375 0 0 0 .375-.375v-4.5a.375.375 0 0 0-.375-.375h-4.5Zm7.875.375c0-1.036.84-1.875 1.875-1.875h4.5C20.16 3 21 3.84 21 4.875v4.5c0 1.036-.84 1.875-1.875 1.875h-4.5a1.875 1.875 0 0 1-1.875-1.875v-4.5Zm1.875-.375a.375.375 0 0 0-.375.375v4.5c0 .207.168.375.375.375h4.5a.375.375 0 0 0 .375-.375v-4.5a.375.375 0 0 0-.375-.375h-4.5ZM6 6.75A.75.75 0 0 1 6.75 6h.75a.75.75 0 0 1 .75.75v.75a.75.75 0 0 1-.75.75h-.75A.75.75 0 0 1 6 7.5v-.75Zm9.75 0A.75.75 0 0 1 16.5 6h.75a.75.75 0 0 1 .75.75v.75a.75.75 0 0 1-.75.75h-.75a.75.75 0 0 1-.75-.75v-.75ZM3 14.625c0-1.036.84-1.875 1.875-1.875h4.5c1.036 0 1.875.84 1.875 1.875v4.5c0 1.035-.84 1.875-1.875 1.875h-4.5A1.875 1.875 0 0 1 3 19.125v-4.5Zm1.875-.375a.375.375 0 0 0-.375.375v4.5c0 .207.168.375.375.375h4.5a.375.375 0 0 0 .375-.375v-4.5a.375.375 0 0 0-.375-.375h-4.5Zm7.875-.75a.75.75 0 0 1 .75-.75h.75a.75.75 0 0 1 .75.75v.75a.75.75 0 0 1-.75.75h-.75a.75.75 0 0 1-.75-.75v-.75Zm6 0a.75.75 0 0 1 .75-.75h.75a.75.75 0 0 1 .75.75v.75a.75.75 0 0 1-.75.75h-.75a.75.75 0 0 1-.75-.75v-.75ZM6 16.5a.75.75 0 0 1 .75-.75h.75a.75.75 0 0 1 .75.75v.75a.75.75 0 0 1-.75.75h-.75a.75.75 0 0 1-.75-.75v-.75Zm9.75 0a.75.75 0 0 1 .75-.75h.75a.75.75 0 0 1 .75.75v.75a.75.75 0 0 1-.75.75h-.75a.75.75 0 0 1-.75-.75v-.75Zm-3 3a.75.75 0 0 1 .75-.75h.75a.75.75 0 0 1 .75.75v.75a.75.75 0 0 1-.75.75h-.75a.75.75 0 0 1-.75-.75v-.75Zm6 0a.75.75 0 0 1 .75-.75h.75a.75.75 0 0 1 .75.75v.75a.75.75 0 0 1-.75.75h-.75a.75.75 0 0 1-.75-.75v-.75Z"" clip-rule=""evenodd"" /></svg>";
        private const string IconShield = @"<svg class=""shrink-0 w-5 h-5 transition duration-75 group-hover:text-fg-brand"" xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"" fill=""currentColor""><path fill-rule=""evenodd"" d=""M12.516 2.17a.75.75 0 0 0-1.032 0 11.209 11.209 0 0 1-7.877 3.08.75.75 0 0 0-.722.515A12.74 12.74 0 0 0 2.25 9.75c0 5.942 4.064 10.933 9.563 12.348a.749.749 0 0 0 .374 0c5.499-1.415 9.563-6.406 9.563-12.348 0-1.39-.223-2.73-.635-3.985a.75.75 0 0 0-.722-.516l-.143.001c-2.996 0-5.717-1.17-7.734-3.08Zm3.094 8.016a.75.75 0 1 0-1.22-.872l-3.236 4.53L9.53 12.22a.75.75 0 0 0-1.06 1.06l2.25 2.25a.75.75 0 0 0 1.14-.094l3.75-5.25Z"" clip-rule=""evenodd"" /></svg>";

        // ==================================================================
        //  MANAGEMENT VIEW — role-based menu shown in default (admin) mode
        // ==================================================================
        private static List<MenuItem> GetManagementMenuItems()
        {
            return new List<MenuItem>
            {
                // ── SuperAdmin ──
                new MenuItem { Title = "Admin Dashboard", Url = "/Admin/Dashboard", Icon = IconDashboard, AllowedRoles = new[] { "SuperAdmin" } },
                new MenuItem { Title = "Manage Companies", Url = "/Admin/Companies", SectionHeader = "Platform Management", Icon = IconBuilding, AllowedRoles = new[] { "SuperAdmin" } },
                new MenuItem { Title = "Manage Users", Url = "/Admin/Users", Icon = IconUsers, AllowedRoles = new[] { "SuperAdmin" } },
                new MenuItem { Title = "Admin Billing", Url = "/Admin/Billing", Icon = IconDocument, AllowedRoles = new[] { "SuperAdmin" } },
                new MenuItem { Title = "Finance", Url = "/Admin/Finance", Icon = IconFinance, AllowedRoles = new[] { "SuperAdmin" } },
                new MenuItem { Title = "System Settings", Url = "/Admin/Settings", Icon = IconSettings, AllowedRoles = new[] { "SuperAdmin" } },
                new MenuItem { Title = "Audit Logs", Url = "/AuditLogs", Icon = IconAudit, AllowedRoles = new[] { "SuperAdmin" } },
                new MenuItem { Title = "Archives", Url = "/Admin/Archives", Icon = IconArchive, AllowedRoles = new[] { "SuperAdmin" } },

                // ── Management Dashboard (Admin, HR, Accountant) ──
                new MenuItem { Title = "Dashboard", Url = "/Dashboard", Icon = IconDashboard, AllowedRoles = new[] { "Admin", "HR", "Accountant" } },

                // ── Organization ──
                new MenuItem { Title = "Employee Management", Url = "/Employees", SectionHeader = "Organization", Icon = IconUsers, AllowedRoles = new[] { "Admin", "HR" } },

                // ── Payroll & Attendance ──
                new MenuItem { Title = "Attendance Monitoring", Url = "/Attendance", SectionHeader = "Payroll & Attendance", Icon = IconClock, AllowedRoles = new[] { "Admin", "HR" } },
                new MenuItem { Title = "Leave Management", Url = "/Admin/LeaveRequests", Icon = IconCalendar, AllowedRoles = new[] { "Admin", "HR", "Accountant" } },
                new MenuItem { Title = "Salary Computation", Url = "/Salary", Icon = IconSalary, AllowedRoles = new[] { "Admin", "Accountant", "HR" } },

                // ── Compensation & Finance ──
                new MenuItem { Title = "Tax & Contributions", Url = "/Contributions", SectionHeader = "Compensation", Icon = IconCard, AllowedRoles = new[] { "Admin", "Accountant" } },
                new MenuItem { Title = "Finance", Url = "/Finance", Icon = IconFinance, AllowedRoles = new[] { "Admin", "Accountant" } },
                new MenuItem { Title = "Compensation", Url = "/Compensation", Icon = IconCompensation, AllowedRoles = new[] { "Admin", "Accountant" } },
                new MenuItem { Title = "Payslip", Url = "/Payslips", Icon = IconPayslip, AllowedRoles = new[] { "Admin", "Accountant" } },

                // ── Administration ──
                new MenuItem { Title = "Billing", Url = "/Billing", SectionHeader = "Administration", Icon = IconCard, AllowedRoles = new[] { "Admin" } },
                new MenuItem { Title = "Audit Logs", Url = "/AuditLogs", Icon = IconAudit, AllowedRoles = new[] { "Admin" } },
                new MenuItem { Title = "Company Settings", Url = "/Settings", Icon = IconSettings, AllowedRoles = new[] { "Admin" } },
                new MenuItem { Title = "Archives", Url = "/Archives", Icon = IconArchive, AllowedRoles = new[] { "Admin" } },
            };
        }

        // ==================================================================
        //  EMPLOYEE VIEW — personal workspace menu for employees
        //  Also shown when HR/Accountant toggles to Employee View
        // ==================================================================
        private static List<MenuItem> GetEmployeeViewMenuItems()
        {
            // AllowedRoles here includes HR and Accountant so they see these
            // items when in Employee View mode.
            return new List<MenuItem>
            {
                new MenuItem { Title = "Dashboard", Url = "/Dashboard", Icon = IconDashboard, AllowedRoles = new[] { "Employee", "HR", "Accountant" } },
                new MenuItem { Title = "My Attendance", Url = "/MyAttendance", SectionHeader = "My Workspace", Icon = IconClock, AllowedRoles = new[] { "Employee", "HR", "Accountant" } },
                new MenuItem { Title = "Leave Request", Url = "/EmployeePortal", Icon = IconCalendar, AllowedRoles = new[] { "Employee", "HR", "Accountant" } },
                new MenuItem { Title = "My Payslips", Url = "/Payslips", Icon = IconPayslip, AllowedRoles = new[] { "Employee", "HR", "Accountant" } },
                new MenuItem { Title = "My Contributions", Url = "/Contributions", Icon = IconCard, AllowedRoles = new[] { "Employee", "HR", "Accountant" } },
                new MenuItem { Title = "My QR Code", Url = "/EmployeeQR", Icon = IconQr, AllowedRoles = new[] { "Employee", "HR", "Accountant" } },
            };
        }

        // ==================================================================
        //  PUBLIC API
        // ==================================================================

        /// <summary>
        /// Returns all menu items (management + employee combined).
        /// Used by GetMenuItemsForRole for backward compat.
        /// </summary>
        public static List<MenuItem> GetMenuItems()
        {
            var items = new List<MenuItem>();
            items.AddRange(GetManagementMenuItems());
            items.AddRange(GetEmployeeViewMenuItems());
            return items;
        }

        /// <summary>
        /// Returns ALL non-SuperAdmin management menu items regardless of role.
        /// Used by _Layout.cshtml when DB RolePermissions are present so that
        /// the Permission Matrix becomes the single source of truth for sidebar visibility.
        /// </summary>
        public static List<MenuItem> GetAllManagementMenuItems()
        {
            return GetManagementMenuItems()
                .Where(item => !(item.AllowedRoles.Length == 1 &&
                    item.AllowedRoles[0].Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        /// <summary>
        /// Returns ALL employee-view menu items regardless of role.
        /// Used by _Layout.cshtml when DB RolePermissions are present so that
        /// the Permission Matrix controls employee sidebar visibility.
        /// </summary>
        public static List<MenuItem> GetAllEmployeeMenuItems()
        {
            return GetEmployeeViewMenuItems();
        }

        /// <summary>
        /// Filters management menu items based on user role.
        /// This is the default view for Admin/HR/Accountant.
        /// </summary>
        public static List<MenuItem> GetMenuItemsForRole(string? userRole)
        {
            if (string.IsNullOrEmpty(userRole))
                return new List<MenuItem>();

            return GetManagementMenuItems()
                .Where(item => item.AllowedRoles.Contains(userRole, StringComparer.OrdinalIgnoreCase))
                .ToList();
        }

        /// <summary>
        /// Returns Employee View menu items filtered by the given role.
        /// When HR/Accountant toggles to Employee View, they see the employee menu.
        /// Pure Employee users also use this.
        /// </summary>
        public static List<MenuItem> GetEmployeeMenuItems(string? userRole = null)
        {
            if (string.IsNullOrEmpty(userRole))
                userRole = "Employee";

            return GetEmployeeViewMenuItems()
                .Where(item => item.AllowedRoles.Contains(userRole, StringComparer.OrdinalIgnoreCase))
                .ToList();
        }

        /// <summary>
        /// Employee page paths that trigger Employee View context.
        /// </summary>
        public static readonly string[] EmployeePagePaths = new[]
        {
            "/EmployeePortal", "/MyAttendance", "/EmployeeQR"
        };

        /// <summary>
        /// Returns true if the given role supports dual-view (Management + Employee).
        /// </summary>
        public static bool IsDualRole(string? role) =>
            !string.IsNullOrEmpty(role) &&
            (role.Equals("HR", StringComparison.OrdinalIgnoreCase) ||
             role.Equals("Accountant", StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Returns a mapping of page URL paths to permission module names.
        /// Built from the management menu items plus known sub-pages.
        /// This is the single source of truth used by PermissionPageFilter.
        /// </summary>
        public static Dictionary<string, string> GetPageToModuleMap()
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in GetManagementMenuItems())
            {
                // Skip SuperAdmin-only items (exempt from permission checks)
                if (item.AllowedRoles.Length == 1 &&
                    item.AllowedRoles[0].Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))
                    continue;

                // Skip Dashboard (always accessible)
                if (item.Url.Equals("/Dashboard", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!map.ContainsKey(item.Url))
                    map[item.Url] = item.Title;
            }

            // Sub-pages that belong to existing modules
            map["/AddEmployee"] = "Employee Management";
            map["/EditEmployee"] = "Employee Management";

            // Employee-view pages
            foreach (var item in GetEmployeeViewMenuItems())
            {
                if (item.Url.Equals("/Dashboard", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (!map.ContainsKey(item.Url))
                    map[item.Url] = item.Title;
            }

            return map;
        }

        /// <summary>
        /// Returns the list of all permission module names (excluding Dashboard).
        /// Used by the startup data-fix to ensure DB records match the menu.
        /// </summary>
        public static string[] GetPermissionModuleNames()
        {
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in GetManagementMenuItems())
            {
                if (item.AllowedRoles.Length == 1 &&
                    item.AllowedRoles[0].Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))
                    continue;
                names.Add(item.Title);
            }
            // Include employee-view module names
            foreach (var item in GetEmployeeViewMenuItems())
            {
                names.Add(item.Title);
            }
            return names.ToArray();
        }
    }
}
