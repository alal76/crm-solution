import {
  QuoteStatusEnum,
  QuoteStatus,
  quoteStatusFromApi,
  quoteStatusToApi,
  OrderStatus,
} from '../types/sales';

describe('QuoteStatus mappings', () => {
  it('roundtrips all numeric values through mapping helpers', () => {
    Object.values(QuoteStatusEnum)
      .filter((v) => typeof v === 'number')
      .forEach((val) => {
        const num = val as number;
        const str = quoteStatusFromApi(num);
        expect(quoteStatusToApi(str)).toBe(num);
      });
  });

  it('defaults unknown API value to Draft and unknown string to Draft numeric', () => {
    expect(quoteStatusFromApi(999)).toBe(QuoteStatus.Draft);
    // @ts-expect-error intentionally passing invalid enum
    expect(quoteStatusToApi('foobar')).toBe(QuoteStatusEnum.Draft);
  });
});

describe('OrderStatus string enum completeness', () => {
  it('contains every status defined in orderService numeric enum', () => {
    // compile-time presence check
    expect(OrderStatus.Draft).toBe('draft');
    expect(OrderStatus.PendingApproval).toBe('pending_approval');
    expect(OrderStatus.Approved).toBe('approved');
    expect(OrderStatus.Processing).toBe('processing');
    expect(OrderStatus.PartiallyFulfilled).toBe('partially_fulfilled');
    expect(OrderStatus.Fulfilled).toBe('fulfilled');
    expect(OrderStatus.Delivered).toBe('delivered');
    expect(OrderStatus.Completed).toBe('completed');
    expect(OrderStatus.Cancelled).toBe('cancelled');
    expect(OrderStatus.Returned).toBe('returned');
    expect(OrderStatus.Refunded).toBe('refunded');
    expect(OrderStatus.OnHold).toBe('on_hold');
    expect(OrderStatus.ActionRequired).toBe('action_required');
  });
});
