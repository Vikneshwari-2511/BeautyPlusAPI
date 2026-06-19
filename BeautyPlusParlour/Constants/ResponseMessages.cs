namespace BeautyPlusParlour.Constants;

public static class ResponseMessages
{
    public const string RegisterSuccess = "Registration successful. Please verify your email.";
    public const string LoginSuccess = "Login successful.";
    public const string LogoutSuccess = "Logged out successfully.";
    public const string LogoutAllSuccess = "Logged out from all devices.";
    public const string EmailVerified = "Email verified successfully.";
    public const string TokenRefreshed = "Token refreshed successfully.";
    public const string TokenRevoked = "Token revoked successfully.";
    public const string OtpSent = "If the email exists, an OTP has been sent.";
    public const string PasswordResetSuccess = "Password reset successfully.";
    public const string InvalidCredentials = "Invalid email or password.";
    public const string EmailNotVerified = "Please verify your email before logging in.";
    public const string EmailAlreadyExists = "An account with this email already exists.";
    public const string InvalidToken = "Invalid or expired token.";
    public const string InvalidOtp = "Invalid or expired OTP.";
    public const string PasswordMismatch = "Passwords do not match.";
    public const string VerificationResent = "Verification email resent successfully.";
    public const string EmailAlreadyVerified = "This email is already verified.";
    public const string SessionsFetched = "Sessions retrieved successfully.";   
    public const string OnSiteDetailsFetched = "On-site details retrieved successfully.";
    // ── Service Management ─────────────────────────────────────────────────────
    public const string CategoryCreated = "Category created successfully.";
    public const string CategoryUpdated = "Category updated successfully.";
    public const string CategoryDeleted = "Category deactivated successfully.";
    public const string CategoryNotFound = "Category not found.";
    public const string CategoryNameExists = "A category with this name already exists.";
    public const string CategoryHasServices = "Cannot deactivate category with active services.";

    public const string SubCategoryCreated = "Sub-category created successfully.";
    public const string SubCategoryUpdated = "Sub-category updated successfully.";
    public const string SubCategoryDeleted = "Sub-category deactivated successfully.";
    public const string SubCategoryNotFound = "Sub-category not found.";
    public const string SubCategoryNameExists = "A sub-category with this name already exists in this category.";
    public const string SubCategoryHasServices = "Cannot deactivate sub-category with active services.";

    public const string ServiceCreated = "Service created successfully.";
    public const string ServiceUpdated = "Service updated successfully.";
    public const string ServiceDeleted = "Service deactivated successfully.";
    public const string ServiceNotFound = "Service not found.";
    public const string ServiceNameExists = "A service with this name already exists.";
    public const string ServiceToggled = "Service status updated successfully.";
    public const string ServicesFetched = "Services retrieved successfully.";
    public const string OnSiteDetailRequired = "OnSite detail is required for OnSite or Both service types.";
    public const string InvalidPriceRange = "Discounted price must be less than base price.";
    // ── Staff Module ──────────────────────────────────────────────────────────
    public const string StaffCreated = "Staff profile created successfully.";
    public const string StaffUpdated = "Staff profile updated successfully.";
    public const string StaffDeleted = "Staff deactivated successfully.";
    public const string StaffNotFound = "Staff profile not found.";
    public const string StaffUserNotFound = "User account not found for staff creation.";
    public const string StaffUserAlreadyExists = "A staff profile already exists for this user.";
    public const string StaffProfileFetched = "Staff profile retrieved successfully.";
    public const string StaffListFetched = "Staff list retrieved successfully.";

    public const string SkillAdded = "Skill added successfully.";
    public const string SkillRemoved = "Skill removed successfully.";
    public const string SkillNotFound = "Staff skill not found.";
    public const string SkillAlreadyExists = "This skill is already mapped to this staff member.";
    public const string SkillsFetched = "Skills retrieved successfully.";

    public const string ScheduleUpdated = "Schedule updated successfully.";
    public const string ScheduleFetched = "Schedule retrieved successfully.";

    public const string LeaveRequested = "Leave request submitted successfully.";
    public const string LeaveApproved = "Leave approved successfully.";
    public const string LeaveRejected = "Leave rejected successfully.";
    public const string LeaveCancelled = "Leave cancelled successfully.";
    public const string LeaveNotFound = "Leave request not found.";
    public const string LeaveAlreadyReviewed = "This leave request has already been reviewed.";
    public const string LeaveDateConflict = "You already have a leave request for this date range.";
    public const string LeaveDatePast = "Cannot request leave for a past date.";
    public const string LeaveRangeExceeded = "Leave range cannot exceed 30 days.";
    public const string AvailableStaffFetched = "Available staff retrieved successfully.";
    // ── Customer Module ───────────────────────────────────────────────────────
    public const string CustomerProfileCreated = "Customer profile created successfully.";
    public const string CustomerProfileUpdated = "Customer profile updated successfully.";
    public const string CustomerProfileFetched = "Customer profile retrieved successfully.";
    public const string CustomerNotFound = "Customer profile not found.";
    public const string CustomerListFetched = "Customer list retrieved successfully.";
    public const string CustomerDeactivated = "Customer deactivated successfully.";

    public const string AddressCreated = "Address added successfully.";
    public const string AddressUpdated = "Address updated successfully.";
    public const string AddressDeleted = "Address removed successfully.";
    public const string AddressNotFound = "Address not found.";
    public const string AddressFetched = "Address retrieved successfully.";
    public const string AddressListFetched = "Addresses retrieved successfully.";
    public const string AddressSetDefault = "Default address updated successfully.";
    public const string AddressLimitReached = "Maximum 5 addresses allowed per customer.";
    public const string AddressCannotDeleteDefault = "Set another address as default before deleting this one.";

    public const string FavouriteAdded = "Service added to favourites.";
    public const string FavouriteRemoved = "Service removed from favourites.";
    public const string FavouritesFetched = "Favourite services retrieved successfully.";
    // ── Booking Module ────────────────────────────────────────────────────────
    public const string BookingCreated = "Booking created successfully.";
    public const string BookingConfirmed = "Booking confirmed successfully.";
    public const string BookingStarted = "Booking marked as in progress.";
    public const string BookingCompleted = "Booking marked as completed.";
    public const string BookingCancelled = "Booking cancelled successfully.";
    public const string BookingRescheduled = "Booking rescheduled successfully.";
    public const string BookingNotFound = "Booking not found.";
    public const string BookingFetched = "Booking retrieved successfully.";
    public const string BookingListFetched = "Bookings retrieved successfully.";
    public const string SlotsFetched = "Available slots retrieved successfully.";
    public const string NoSlotsAvailable = "No available slots for the selected date.";
    public const string SlotNotAvailable = "Selected slot is not available.";
    public const string BookingCannotBeCancelled = "Completed bookings cannot be cancelled.";
    public const string BookingCannotReschedule = "Only pending or confirmed bookings can be rescheduled.";
    public const string BookingAddressRequired = "Address is required for on-site bookings.";
    public const string BookingItemsRequired = "At least one service item is required.";
    public const string BookingItemLimitExceeded = "Maximum 5 services per booking.";
    public const string StaffSkillNotFound = "Selected staff cannot perform one or more selected services.";
    public const string AdvancePaymentRequired = "Advance payment must be recorded before confirming on-site booking.";
    public const string PaymentRecorded = "Payment recorded successfully.";
    public const string PaymentsFetched = "Payments retrieved successfully.";
    public const string ConsultationScheduled = "Consultation call scheduled successfully.";
    public const string ConsultationCompleted = "Consultation marked as completed.";
    // ── Loyalty Module ────────────────────────────────────────────────────────
    public const string LoyaltyPointsFetched = "Loyalty points retrieved successfully.";
    public const string LoyaltyTransactionsFetched = "Transaction history retrieved successfully.";
    public const string LoyaltyAdjusted = "Loyalty points adjusted successfully.";
    public const string LoyaltyCustomersFetched = "Customer loyalty data retrieved successfully.";
    public const string InsufficientPoints = "Insufficient loyalty points.";
    public const string MinRedeemPointsError = "Minimum 50 points required to redeem.";
    public const string MaxRedeemExceeded = "Cannot redeem more than 20% of booking total via points.";
    public const string ValidateRedeemSuccess = "Points can be redeemed.";

    // ── Coupon Module ─────────────────────────────────────────────────────────
    public const string CouponCreated = "Coupon created successfully.";
    public const string CouponUpdated = "Coupon updated successfully.";
    public const string CouponDeactivated = "Coupon deactivated successfully.";
    public const string CouponNotFound = "Coupon not found.";
    public const string CouponCodeExists = "A coupon with this code already exists.";
    public const string CouponValid = "Coupon is valid.";
    public const string CouponInvalid = "Invalid or expired coupon code.";
    public const string CouponExpired = "This coupon has expired.";
    public const string CouponNotActive = "This coupon is not active.";
    public const string CouponUsageLimitReached = "This coupon has reached its maximum usage limit.";
    public const string CouponPerUserLimitReached = "You have already used this coupon the maximum number of times.";
    public const string CouponMinOrderNotMet = "Order amount does not meet the minimum required for this coupon.";
    public const string CouponsFetched = "Coupons retrieved successfully.";
    public const string CouponUsageFetched = "Coupon usage retrieved successfully.";
    // ── Reviews Module ────────────────────────────────────────────────────────
    public const string ReviewCreated = "Review submitted successfully.";
    public const string ReviewUpdated = "Review updated successfully.";
    public const string ReviewHidden = "Review hidden successfully.";
    public const string ReviewUnhidden = "Review made visible successfully.";
    public const string ReviewNotFound = "Review not found.";
    public const string ReviewAlreadyExists = "You have already reviewed this booking.";
    public const string ReviewBookingNotCompleted = "You can only review completed bookings.";
    public const string ReviewNotYourBooking = "You can only review your own bookings.";
    public const string ReviewEditExpired = "Reviews can only be edited within 24 hours of submission.";
    public const string ReviewsFetched = "Reviews retrieved successfully.";
    public const string ReviewFetched = "Review retrieved successfully.";
    // ── Notifications Module ──────────────────────────────────────────────────
    public const string NotificationsFetched = "Notifications retrieved successfully.";
    public const string NotificationMarkedRead = "Notification marked as read.";
    public const string AllNotificationsRead = "All notifications marked as read.";
    public const string NotificationDeleted = "Notification deleted successfully.";
    public const string NotificationNotFound = "Notification not found.";
    // ── Dashboard Module ──────────────────────────────────────────────────────
    public const string DashboardSummaryFetched = "Dashboard summary retrieved successfully.";
    public const string RevenueDataFetched = "Revenue data retrieved successfully.";
    public const string BookingAnalyticsFetched = "Booking analytics retrieved successfully.";
    public const string TopServicesFetched = "Top services retrieved successfully.";
    public const string TopStaffFetched = "Top staff retrieved successfully.";
    public const string RecentBookingsFetched = "Recent bookings retrieved successfully.";
    public const string CustomerAnalyticsFetched = "Customer analytics retrieved successfully.";
    public const string LoyaltyAnalyticsFetched = "Loyalty analytics retrieved successfully.";
    // ── Firebase Auth ─────────────────────────────────────────
    public const string FirebaseTokenInvalid = "Invalid Firebase token.";
    public const string FirebaseLoginSuccess = "Phone login successful.";
    public const string FirebaseAccountCreated = "Account created via phone login.";
    // ── Common ─────────────────────────────────────────
    public const string AccessDenied = "You do not have permission to perform this action.";
}