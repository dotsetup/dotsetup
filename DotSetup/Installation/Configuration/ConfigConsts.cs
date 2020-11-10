// Copyright (c) dotSetup. All Rights Reserved.
// Licensed under the GPL License, version 3.0.
// https://dotsetup.io/

namespace DotSetup
{
    public static class ConfigConsts
    {
        public static readonly string
            ACNM = "ACNM",
            PRODUCT = "PRODUCT",
            PROD_CODE = "PROD_CODE",
            PRODUCT_TITLE = "PRODUCT_TITLE",
            URL_ANALYTICS = "URL_ANALYTICS",
            URL_REMOTE_CONFIG = "URL_REMOTE_CONFIG",
            URL_DYNAMIC_CONFIG = "URL_DYNAMIC_CONFIG",
            PRODUCT_VARIANT = "PRODUCT_VARIANT"
            ;
            


        public static readonly string[] ReportMandatory = { ACNM, PRODUCT, PRODUCT_TITLE, URL_ANALYTICS, URL_REMOTE_CONFIG };
        public static readonly string[] SensitiveConfigKeys = { ACNM, PRODUCT, URL_ANALYTICS, URL_REMOTE_CONFIG };
    }
}
