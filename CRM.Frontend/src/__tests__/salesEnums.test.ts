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

describe('OrderStatus numeric enum completeness', () => {
  it('contains every status defined in the numeric enum', () => {
    // compile-time presence check against actual numeric enum
    expect(OrderStatus.Draft).toBe(0);
    expect(OrderStatus.Submitted).toBe(1);
    expect(OrderStatus.Pending).toBe(2);
    expect(OrderStatus.Processing).toBe(3);
    expect(OrderStatus.Approved).toBe(4);
    expect(OrderStatus.OnHold).toBe(5);
    expect(OrderStatus.Shipped).toBe(6);
    expect(OrderStatus.Delivered).toBe(7);
    expect(OrderStatus.Completed).toBe(8);
    expect(OrderStatus.Cancelled).toBe(9);
    expect(OrderStatus.Refunded).toBe(10);
    expect(OrderStatus.Returned).toBe(11);
    expect(OrderStatus.ActionRequired).toBe(12);
  });
});
