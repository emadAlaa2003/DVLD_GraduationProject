# 06 — الموبايل وحسابات المواطنين

محدّث في 2026-10-09 وفق `main` عند `2dc6cab`. لا يشمل تعديلات تفاصيل الرخص المحلية غير المرفوعة، ولا يمثل نتيجة اختبار تشغيل.

## إدارة الحساب من الكمبيوتر

[frmMobileAccount.cs](../../DVLD/People/frmMobileAccount.cs) تُفتح من قائمة الأشخاص عبر `frmMobileAccount(PersonID)` و`ShowDialog()`. `_LoadData` يقرأ الشخص وحسابه، و`_RefreshAccountState` يضبط No Account/Active/Inactive والأزرار والصور.

`btnCreateAccount_Click` يستخدم `NationalNo.Trim()` للاسم، وكلمة المرور المؤقتة `FirstName.Trim() + NationalNo.Trim() + "!"` ثم يستدعي `clsMobileUser.HashPassword` و`Create`. إعادة التعيين تستخدم الصيغة نفسها؛ التفعيل والتعطيل يستدعيان `SetIsActive`. الصيغة قابلة للتوقع ومقصودة للنموذج الحالي، وليست سياسة كلمات مرور لنشر عام.

[clsMobileUser.cs](../../DVLD_Buisness/clsMobileUser.cs) يقدم `FindByUsername` و`FindByMobileUserID` و`FindByPersonID` و`Create` و`ResetPasswordHash` و`SetIsActive`. الهاش بصيغة Identity V3: PBKDF2/SHA512، مئة ألف تكرار، Salt عشوائي 16 بايت وSubkey 32 بايت مع رأس بصيغة network byte order. لا توجد حاجة إلى إضافة Identity package لمشروع Business ذي هدف net48.

[clsMobileUserData.cs](../../DVLD_DataAccess/clsMobileUserData.cs) يستخدم SQL parameters و`using` ولا يخفي استثناءات SQL. [MobileUsers_Migration.sql](../../Database/Migrations/MobileUsers_Migration.sql) ينشئ MobileUsers مع FK إلى People، وUnique للشخص وUsername، واسم غير فارغ ودون المسافة العادية، وPasswordHash غير فارغ وIsActive الافتراضي 1. Username ذو collation غير حساس لحالة الأحرف. حسابات المواطنين منفصلة عن Users الموظفين.

## تسجيل الدخول وجلسة Cookie

[MobileAuthController.Login](../../DVLD.Api/Controllers/MobileAuthController.cs) يستقبل `username` و`password` عبر `POST /api/mobile-auth/login`. يبحث عن الحساب ويفحص الهاش بواسطة `PasswordHasher<clsMobileUser>`، ثم يعيد:

- 401 برسالة عامة عند اسم/كلمة مرور غير صحيحين.
- 403 إذا كانت كلمة المرور صحيحة والحساب غير فعّال.
- 200 مع `personId` و`fullName` و`username` وإصدار Cookie عند النجاح.

لا يصدر JWT. claims تشمل NameIdentifier لهوية MobileUserID، و`person_id`، و`account_type=mobile_citizen`.

[Program.cs](../../DVLD.Api/Program.cs) يربط scheme `DVLD.MobileCitizen` وCookie `DVLD.MobileAuth`، وHttpOnly وSameSite Strict. المدة مصدرها MobileAuth:SessionHours، وافتراضيها 8 ساعات، دون SlidingExpiration. SecurePolicy هي SameAsRequest في Development وAlways خارجه.

[MobileCookieAuthenticationEvents.ValidatePrincipal](../../DVLD.Api/Authentication/MobileCookieAuthenticationEvents.cs) يقرأ الحساب مجددًا لكل طلب يحمل Cookie ويتحقق من claims ووجود الحساب ونشاطه وتطابق PersonID. يرفض الهوية ويعمل SignOut عند عدم التطابق. تعطيل الحساب يمنع طلباته التالية؛ تغيير PasswordHash وحده لا يبطل Cookie سابقة، وإعادة تفعيل الحساب قد تسمح بها حتى انتهاء مدتها.

## ملكية بيانات المواطن

Controllers [الرخص](../../DVLD.Api/Controllers/LicensesController.cs)، [الرخص الدولية](../../DVLD.Api/Controllers/InternationalLicensesController.cs)، [الطلبات](../../DVLD.Api/Controllers/ApplicationsController.cs) و[المواعيد](../../DVLD.Api/Controllers/TestAppointmentsController.cs) عليها Authorize للـscheme المذكور.

القوائم تقارن personId في المسار مع claim قبل القراءة. التفاصيل تقارن صاحب الطلب، أو DriverInfo.PersonID للرخص، أو ApplicantPersonID للطلب المحلي المرتبط بالموعد. النتيجة دون مصادقة 401، وسجل شخص آخر 404، وبيانات صاحب الحساب تُعاد وفق شروط وجود السجل الطبيعية.

PeopleController يستخدم EmployeeApiOnly: الزائر 401 والمواطن 403. السياسة الحالية تتعمد إغلاقه للجميع حتى تُنفّذ مصادقة الموظف للـAPI. هذا لا يمنع WinForms من استدعاء Business مباشرة. OfficialExamsController لا يطلب Cookie المواطن، ولا توجد Fallback Policy تفرضها عليه؛ نطاقه مركز الامتحان وليس تطبيق المواطن.

## بنية Android وتدفق البيانات

المصدر تحت [DVLD.Mobile/app/src/main/java/com/dvld/mobile](../../DVLD.Mobile/app/src/main/java/com/dvld/mobile/):

| الملف | الوظيفة |
|---|---|
| `MainActivity.java` | واجهة دخول المواطن والانتقال بعد نجاحه |
| `network/MobileAuthApiService.java` | عقد POST لتسجيل الدخول |
| `repository/ApiAuthRepository.java` | إرسال الطلب غير المتزامن، التحقق من الرد وربط 401/403 وأخطاء الشبكة/الخادم برسائل الواجهة |
| `network/ApiClient.java` | Retrofit/Gson وOkHttp مشترك بين خدمات الدخول والبيانات |
| `network/SessionCookieJar.java` | تخزين Cookies بالذاكرة، استبدالها وحذف المنتهية وإرسال ما يطابق الرابط، بما فيها cookies المجزأة |
| `ui/DashboardActivity.java` | عرض الملخص والتحديث عند العودة للشاشة والانتقال إلى رخصي |
| `repository/ApiDashboardRepository.java` | جمع قوائم الرخص والطلبات والمواعيد للملخص |
| `network/DashboardApiService.java` | GET لقوائم الرخص المحلية والدولية والطلبات والمواعيد |
| `ui/MyLicensesActivity.java` و`repository/ApiLicensesRepository.java` | عرض قوائم الرخص المحلية والدولية من API |
| `ui/DashboardTextMapper.java` و`ui/LicenseCardTextMapper.java` | تحويل البيانات لنصوص العرض |

`ApiAuthRepository.login` يرسل username بعد Trim ولا يقص كلمة المرور. يتحقق من body عند 200، ويرجع INVALID_CREDENTIALS لـ401 وINACTIVE_ACCOUNT لـ403. الاستدعاءات قابلة للإلغاء؛ لا يعرض نص خطأ الخادم الخام أو يسجل بيانات الدخول في هذا المسار.

اسم الشخص وPersonID يُمرران للشاشات لعرض البيانات وطلبها، لكن الخادم يعيد التحقق من الملكية. Cookie لا تُحفظ إلى القرص؛ عمرها داخل تطبيق Android مرتبط بعمر العملية، حتى لو انتهاؤها على الخادم أطول.

## التشغيل المحلي والنطاق المتبقي

افتح مجلد DVLD.Mobile في Android Studio واعمل Gradle Sync. الإعداد compileSdk/targetSdk 36 وminSdk 26 وJava compatibility 11. عنوان العميل الحالي `http://127.0.0.1:5277/` ويحتاج:

```powershell
dotnet run --project .\DVLD.Api\DVLD.Api.csproj --launch-profile http
adb reverse tcp:5277 tcp:5277
```

هذا إعداد Debug/Development المحلي. إعداد الشبكة في `app/src/debug` يسمح بـHTTP المحلي، بينما Release لا يسمح به. لا تعطّل التحقق من شهادة HTTPS كحل للنشر؛ التشغيل خارج هذا السيناريو يحتاج عنوان خادم وشهادة وإعداد اتصال مناسبين.

نفّذ إنشاء/إعادة تعيين الحساب من WinForms بعد تجهيز DVLD_CONNECTION_STRING وقاعدة تشمل MobileUsers. لا تضف بيانات دخول حقيقية إلى الدليل أو Git.

في النسخة الموثقة يوجد Login وDashboard وقوائم رخصي. وجود أزرار للمساعد أو التدريب أو غيرهما لا يثبت اكتمال شاشاتها. تعديلات شاشة تفاصيل الرخص المحلية في مساحة العمل ليست جزءًا من commit المرجعي لهذه الوثيقة.

للتأكد وقت التسليم: جرّب حسابًا فعالًا، بيانات خاطئة، حسابًا معطّلًا، انتهاء/فقد Cookie، بيانات مواطن آخر، وإعادة تحميل القوائم بعد تغييرات Desktop. هذه قائمة تحقق، وليست ادعاءً بتنفيذ تلك التجارب أثناء التوثيق.
